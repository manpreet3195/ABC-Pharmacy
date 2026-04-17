using ABCPharmacy.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPharmacyServices(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPharmacyEndpoints();

app.Run();
