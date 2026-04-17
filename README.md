# ABC Pharmacy Medicine Tracker
Single Page Application with ASP.NET Core Web API and React.js for tracking medicines and sale records.

## Architecture updates
- React.js frontend (CDN-based) in `wwwroot/app.jsx`
- Configuration-driven JSON paths via `appsettings.json` (`JsonStore` section)
- Dependency Injection registration through extension methods
- HTTP endpoint logic moved into handler classes (SOLID-friendly separation)
- Validation moved to dedicated validator classes
- HTTP-only launch profile (HTTPS URL removed)

## Features
- View medicine inventory in a grid (without Notes column)
- Add medicine details
- Color indications:
  - Red rows for expiry date within 30 days
  - Yellow rows for quantity less than 10
- Search medicines by full name
- Record medicine sales and maintain sale history
- Data persisted in JSON files on server side

## Project structure
- `Program.cs` - app bootstrap only
- `Extensions/` - dependency registration and endpoint mapping
- `Handlers/` - HTTP verb handlers
- `Validators/` - request validators
- `Options/JsonStoreOptions.cs` - strongly typed configuration
- `Services/PharmacyStore.cs` - JSON persistence implementation
- `wwwroot/index.html`, `wwwroot/app.jsx`, `wwwroot/styles.css` - React SPA
- `appsettings.json` - configurable storage file paths

## Run
1. Install .NET SDK 8.0+.
2. Open a terminal in `C:\Users\user\source\repos\ABC-Pharmacy`.
3. Run:
   - `dotnet run --project .\ABCPharmacy.csproj`
4. Open the URL shown in terminal (`http://localhost:50152` by default).
