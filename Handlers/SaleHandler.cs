using ABCPharmacy.Models;
using ABCPharmacy.Services;
using ABCPharmacy.Validators;

namespace ABCPharmacy.Handlers;

public sealed class SaleHandler
{
    private readonly IPharmacyStore _store;
    private readonly IValidator<CreateSaleRequest> _validator;

    public SaleHandler(IPharmacyStore store, IValidator<CreateSaleRequest> validator)
    {
        _store = store;
        _validator = validator;
    }

    public async Task<IResult> GetSalesAsync()
    {
        var sales = await _store.GetSalesAsync();
        return Results.Ok(sales
            .OrderByDescending(s => s.SoldAt)
            .ToList());
    }

    public async Task<IResult> RecordSaleAsync(CreateSaleRequest request)
    {
        var errors = _validator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(new { errors });
        }

        try
        {
            var sale = await _store.RecordSaleAsync(request.MedicineId, request.QuantitySold);
            return Results.Ok(sale);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
