using ABCPharmacy.Models;
using ABCPharmacy.Services;
using ABCPharmacy.Validators;

namespace ABCPharmacy.Handlers;

public sealed class MedicineHandler
{
    private readonly IPharmacyStore _store;
    private readonly IValidator<CreateMedicineRequest> _validator;

    public MedicineHandler(IPharmacyStore store, IValidator<CreateMedicineRequest> validator)
    {
        _store = store;
        _validator = validator;
    }

    public async Task<IResult> GetMedicinesAsync(string? search)
    {
        var medicines = await _store.GetMedicinesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            medicines = medicines
                .Where(m => m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        medicines = medicines
            .OrderBy(m => m.FullName)
            .ToList();

        return Results.Ok(medicines);
    }

    public async Task<IResult> AddMedicineAsync(CreateMedicineRequest request)
    {
        var errors = _validator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(new { errors });
        }

        var medicine = new Medicine
        {
            FullName = request.FullName.Trim(),
            Notes = request.Notes?.Trim(),
            ExpiryDate = request.ExpiryDate.Date,
            Quantity = request.Quantity,
            Price = Math.Round(request.Price, 2),
            Brand = request.Brand.Trim()
        };

        var created = await _store.AddMedicineAsync(medicine);
        return Results.Created($"/api/medicines/{created.Id}", created);
    }
}
