# ABC Pharmacy Medicine Tracker
Single Page Application with ASP.NET Core Web API + vanilla JavaScript for tracking medicines and sale records.

## Features
- View medicine inventory in a grid (without Notes column)
- Add medicine details
- Color indications:
  - Red rows for expiry date within 30 days
  - Yellow rows for quantity less than 10
- Search medicines by full name
- Record medicine sales and maintain sale history
- Data persisted in server-side JSON files under `App_Data`

## Project Structure
- `Program.cs` - API endpoints and static file hosting
- `Services/PharmacyStore.cs` - JSON persistence and business logic
- `Models/` - request and entity models
- `wwwroot/` - SPA UI files (`index.html`, `styles.css`, `app.js`)
- `App_Data/` - JSON storage files

## Run Instructions
1. Install .NET SDK (8.0 or later).
2. From the project folder, run:
   - `dotnet run --project .\ABCPharmacy.csproj`
3. Open the URL shown in the terminal (typically `http://localhost:5000`) in Preview/browser.
