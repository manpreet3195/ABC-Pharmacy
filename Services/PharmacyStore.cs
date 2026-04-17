using System.Text.Json;
using ABCPharmacy.Models;
using ABCPharmacy.Options;
using Microsoft.Extensions.Options;

namespace ABCPharmacy.Services;

public sealed class PharmacyStore : IPharmacyStore  
{
    private readonly string _medicinesFilePath;
    private readonly string _salesFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public PharmacyStore(IHostEnvironment env, IOptions<JsonStoreOptions> options)
    {
        var configured = options.Value;
        _medicinesFilePath = ResolveFilePath(env.ContentRootPath, configured.MedicinesFilePath);
        _salesFilePath = ResolveFilePath(env.ContentRootPath, configured.SalesFilePath);

        EnsureStorageFile(_medicinesFilePath);
        EnsureStorageFile(_salesFilePath);
    }

    public async Task<List<Medicine>> GetMedicinesAsync()
    {
        return await ReadListAsync<Medicine>(_medicinesFilePath);
    }

    public async Task<Medicine> AddMedicineAsync(Medicine medicine)
    {
        var medicines = await ReadListAsync<Medicine>(_medicinesFilePath);
        medicine.Id = Guid.NewGuid();
        medicine.Price = Math.Round(medicine.Price, 2);
        medicines.Add(medicine);
        await WriteListAsync(_medicinesFilePath, medicines);
        return medicine;
    }

    public async Task<List<SaleRecord>> GetSalesAsync()
    {
        return await ReadListAsync<SaleRecord>(_salesFilePath);
    }

    public async Task<SaleRecord> RecordSaleAsync(Guid medicineId, int quantitySold)
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

    private static string ResolveFilePath(string contentRootPath, string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("JSON store file path cannot be empty.");
        }

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }

    private static void EnsureStorageFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<T>> ReadListAsync<T>(string filePath)
    {
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
