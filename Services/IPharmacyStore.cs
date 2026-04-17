using ABCPharmacy.Models;

namespace ABCPharmacy.Services;

public interface IPharmacyStore
{
    Task<List<Medicine>> GetMedicinesAsync();
    Task<Medicine> AddMedicineAsync(Medicine medicine);
    Task<List<SaleRecord>> GetSalesAsync();
    Task<SaleRecord> RecordSaleAsync(Guid medicineId, int quantitySold);
}
