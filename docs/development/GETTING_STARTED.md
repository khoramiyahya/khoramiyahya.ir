# Getting started

## Prerequisites

- .NET SDK 10
- SQL Server or SQL Server LocalDB
- Repository-local EF Core tool (`dotnet tool restore`)

## Run the API

Development uses SQL Server LocalDB by default. Configure a local JWT signing key before starting the API:

```bash
dotnet user-secrets -p src/PersonalBrand.WebAPI set "Jwt:SigningKey" "<cryptographically-random-256-bit-secret>"
```

To use a different SQL Server instance, store its connection string in user secrets instead of tracked application settings:

```bash
dotnet user-secrets -p src/PersonalBrand.WebAPI set "ConnectionStrings:DefaultConnection" "<sql-server-connection-string>"
```

The optional bootstrap administrator must also use user secrets:

```bash
dotnet user-secrets -p src/PersonalBrand.WebAPI set "BootstrapAdmin:Email" "<admin-email>"
dotnet user-secrets -p src/PersonalBrand.WebAPI set "BootstrapAdmin:Password" "<strong-unique-password>"
```

Remove both bootstrap administrator secrets after the account is created successfully. They are not a password-rotation mechanism.

```bash
dotnet restore
dotnet tool restore
dotnet build
dotnet run --project src/PersonalBrand.WebAPI --launch-profile https
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
- `BootstrapAdmin__Email` (optional)
- `BootstrapAdmin__Password` (optional)

Do not place credentials in any tracked `appsettings*.json` file. Rotate any credential that has previously been committed because deleting it from the current file does not remove it from Git history.

Production startup fails before database initialization when SQL Server, JWT, CORS, or bootstrap-administrator configuration is missing or unsafe.

## Tests

```bash
dotnet test
```
