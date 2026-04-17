using ABCPharmacy.Handlers;
using ABCPharmacy.Models;
using ABCPharmacy.Options;
using ABCPharmacy.Services;
using ABCPharmacy.Validators;

namespace ABCPharmacy.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPharmacyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JsonStoreOptions>(configuration.GetSection(JsonStoreOptions.SectionName));
        services.AddSingleton<IPharmacyStore, PharmacyStore>();

        services.AddSingleton<IValidator<CreateMedicineRequest>, CreateMedicineRequestValidator>();
        services.AddSingleton<IValidator<CreateSaleRequest>, CreateSaleRequestValidator>();

        services.AddScoped<MedicineHandler>();
        services.AddScoped<SaleHandler>();

        return services;
    }
}
