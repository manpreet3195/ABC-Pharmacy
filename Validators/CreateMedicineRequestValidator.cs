using ABCPharmacy.Models;

namespace ABCPharmacy.Validators;

public sealed class CreateMedicineRequestValidator : IValidator<CreateMedicineRequest>
{
    public IReadOnlyList<string> Validate(CreateMedicineRequest instance)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(instance.FullName))
        {
            errors.Add("Full Name is required.");
        }

        if (string.IsNullOrWhiteSpace(instance.Brand))
        {
            errors.Add("Brand is required.");
        }

        if (instance.ExpiryDate == default)
        {
            errors.Add("Expiry Date is required.");
        }

        if (instance.Quantity < 0)
        {
            errors.Add("Quantity cannot be negative.");
        }

        if (instance.Price < 0)
        {
            errors.Add("Price cannot be negative.");
        }

        if (GetDecimalScale(instance.Price) > 2)
        {
            errors.Add("Price can have at most 2 decimal places.");
        }

        return errors;
    }

    private static int GetDecimalScale(decimal value)
    {
        var bits = decimal.GetBits(value);
        return (bits[3] >> 16) & 0x7F;
    }
}
