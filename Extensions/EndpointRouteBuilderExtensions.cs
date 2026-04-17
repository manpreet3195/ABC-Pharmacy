using ABCPharmacy.Handlers;
using ABCPharmacy.Models;

namespace ABCPharmacy.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPharmacyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/medicines", (string? search, MedicineHandler handler) =>
            handler.GetMedicinesAsync(search));

        endpoints.MapPost("/api/medicines", (CreateMedicineRequest request, MedicineHandler handler) =>
            handler.AddMedicineAsync(request));

        endpoints.MapGet("/api/sales", (SaleHandler handler) =>
            handler.GetSalesAsync());

        endpoints.MapPost("/api/sales", (CreateSaleRequest request, SaleHandler handler) =>
            handler.RecordSaleAsync(request));

        return endpoints;
    }
}
