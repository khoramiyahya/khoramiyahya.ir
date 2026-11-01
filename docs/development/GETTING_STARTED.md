# Getting started

## Prerequisites

- .NET SDK 10
- SQL Server or SQL Server LocalDB
- Optional: EF Core global tool (`dotnet-ef`)

## Run the API

```bash
dotnet restore
dotnet build
dotnet run --project src/PersonalBrand.WebAPI
```

Swagger UI: `https://localhost:7080/swagger`  
Status endpoint: `https://localhost:7080/api/v1/status`  
Health endpoint: `https://localhost:7080/health`

## Configuration

Primary files:

- `src/PersonalBrand.WebAPI/appsettings.json`
- `src/PersonalBrand.WebAPI/appsettings.Development.json`
- `src/PersonalBrand.WebAPI/appsettings.Production.json`

Sensitive Production values should come from environment variables or the hosting control panel:

- `ConnectionStrings__DefaultConnection`
- `Jwt__SigningKey`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Cors__AllowedOrigins__0`

## Tests

```bash
dotnet test
```
