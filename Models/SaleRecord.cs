namespace ABCPharmacy.Models;

public sealed class SaleRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime SoldAt { get; set; } = DateTime.UtcNow;
}
