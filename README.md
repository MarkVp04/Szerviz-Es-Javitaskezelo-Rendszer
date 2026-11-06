# Szerviz- és javításkezelő rendszer

Egy kisebb műszaki szerviz javítási folyamatainak, ügyfeleinek és eszközeinek nyilvántartására készült egyetemi projekt.

## Projektek

- **SzervizApp.Api** – ASP.NET Core REST API, JWT-hitelesítés és jogosultságkezelés.
- **SzervizApp.Services** – üzleti logika és DTO-k.
- **SzervizApp.DataContext** – Entity Framework Core adatmodell, migrációk és SQLite.
- **SzervizApp.Client** – Windows Forms asztali kliens.

## Indítás

A backend indítása:

```powershell
dotnet run --project .\SzervizApp.Api\SzervizApp.Api.csproj
```

A kliens indítása egy másik terminálból:

```powershell
dotnet run --project .\SzervizApp.Client\SzervizApp.Client.csproj
```

A kliens alapértelmezett API-címe `http://localhost:5203`, amely a bejelentkezési ablakban módosítható. További indítási információk a [SzervizApp.Client README-jében](./SzervizApp.Client/README.md) találhatók.

## Technológia

- .NET 9
- ASP.NET Core Web API
- Windows Forms
- Entity Framework Core 9
- SQLite
- JWT
