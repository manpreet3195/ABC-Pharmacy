using ABCPharmacy.Models;

namespace ABCPharmacy.Validators;

public sealed class CreateSaleRequestValidator : IValidator<CreateSaleRequest>
{
    public IReadOnlyList<string> Validate(CreateSaleRequest instance)
    {
        var errors = new List<string>();

        if (instance.MedicineId == Guid.Empty)
        {
            errors.Add("Medicine is required.");
        }

        if (instance.QuantitySold <= 0)
        {
            errors.Add("Quantity sold must be greater than 0.");
        }

        return errors;
    }
}
