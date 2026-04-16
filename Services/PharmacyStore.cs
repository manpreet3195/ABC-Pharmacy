using System.Text.Json;
using ABCPharmacy.Models;

namespace ABCPharmacy.Services;

public sealed class PharmacyStore
{
    private readonly string _medicinesFilePath;
    private readonly string _salesFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public PharmacyStore(IHostEnvironment env)
    {
        var dataDirectory = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _medicinesFilePath = Path.Combine(dataDirectory, "medicines.json");
        _salesFilePath = Path.Combine(dataDirectory, "sales.json");
    }

    public async Task<List<Medicine>> GetMedicinesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await ReadListAsync<Medicine>(_medicinesFilePath);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Medicine> AddMedicineAsync(Medicine medicine)
    {
        await _gate.WaitAsync();
        try
        {
            var medicines = await ReadListAsync<Medicine>(_medicinesFilePath);
            medicine.Id = Guid.NewGuid();
            medicine.Price = Math.Round(medicine.Price, 2);
            medicines.Add(medicine);
            await WriteListAsync(_medicinesFilePath, medicines);
            return medicine;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<List<SaleRecord>> GetSalesAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await ReadListAsync<SaleRecord>(_salesFilePath);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<SaleRecord> RecordSaleAsync(Guid medicineId, int quantitySold)
    {
        await _gate.WaitAsync();
        try
        {
            var medicines = await ReadListAsync<Medicine>(_medicinesFilePath);
            var medicine = medicines.FirstOrDefault(m => m.Id == medicineId);

            if (medicine is null)
            {
                throw new KeyNotFoundException("Medicine not found.");
            }

            if (medicine.Quantity < quantitySold)
            {
                throw new InvalidOperationException("Insufficient stock for this sale.");
            }

            medicine.Quantity -= quantitySold;
            await WriteListAsync(_medicinesFilePath, medicines);

            var sales = await ReadListAsync<SaleRecord>(_salesFilePath);
            var sale = new SaleRecord
            {
                Id = Guid.NewGuid(),
                MedicineId = medicine.Id,
                MedicineName = medicine.FullName,
                QuantitySold = quantitySold,
                UnitPrice = Math.Round(medicine.Price, 2),
                SoldAt = DateTime.UtcNow
            };

            sales.Add(sale);
            await WriteListAsync(_salesFilePath, sales);

            return sale;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<T>> ReadListAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, "[]");
            return new List<T>();
        }

        await using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var records = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
        return records ?? new List<T>();
    }

    private async Task WriteListAsync<T>(string filePath, List<T> data)
    {
        await using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
    }
}
