using ABCPharmacy.Models;
using ABCPharmacy.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PharmacyStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/medicines", async (string? search, PharmacyStore store) =>
{
    var medicines = await store.GetMedicinesAsync();

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
});

app.MapPost("/api/medicines", async (CreateMedicineRequest request, PharmacyStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName))
    {
        return Results.BadRequest(new { error = "Full Name is required." });
    }

    if (string.IsNullOrWhiteSpace(request.Brand))
    {
        return Results.BadRequest(new { error = "Brand is required." });
    }

    if (request.Quantity < 0)
    {
        return Results.BadRequest(new { error = "Quantity cannot be negative." });
    }

    if (request.Price < 0)
    {
        return Results.BadRequest(new { error = "Price cannot be negative." });
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

    var created = await store.AddMedicineAsync(medicine);
    return Results.Created($"/api/medicines/{created.Id}", created);
});

app.MapGet("/api/sales", async (PharmacyStore store) =>
{
    var sales = await store.GetSalesAsync();
    return Results.Ok(sales
        .OrderByDescending(s => s.SoldAt)
        .ToList());
});

app.MapPost("/api/sales", async (CreateSaleRequest request, PharmacyStore store) =>
{
    if (request.MedicineId == Guid.Empty)
    {
        return Results.BadRequest(new { error = "Medicine is required." });
    }

    if (request.QuantitySold <= 0)
    {
        return Results.BadRequest(new { error = "Quantity sold must be greater than 0." });
    }

    try
    {
        var sale = await store.RecordSaleAsync(request.MedicineId, request.QuantitySold);
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
});

app.Run();
