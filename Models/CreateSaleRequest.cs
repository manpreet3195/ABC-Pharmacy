namespace ABCPharmacy.Models;

public sealed class CreateSaleRequest
{
    public Guid MedicineId { get; set; }
    public int QuantitySold { get; set; }
}
