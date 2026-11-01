# Yahya KHorami Personal Brand Platform

Production backend for the Yahya KHorami personal brand platform.

## Technology stack

- .NET 10 LTS
- ASP.NET Core Web API
- Clean Architecture + Modular Monolith
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity + JWT Bearer
- FluentValidation
- Serilog
- Swagger / OpenAPI
- xUnit + ASP.NET Core MVC Testing

## Solution architecture

```text
src/
  PersonalBrand.Domain
  PersonalBrand.Application
  PersonalBrand.Infrastructure
  PersonalBrand.WebAPI
tests/
  PersonalBrand.UnitTests
  PersonalBrand.IntegrationTests
docs/
database/
```

Dependency rule: `WebAPI → Application → Domain`, with `Infrastructure` implementing Application abstractions.

See [docs/architecture/ARCHITECTURE.md](docs/architecture/ARCHITECTURE.md).

## Prerequisites

- .NET SDK 10 (`global.json` pins `10.0.403`)
- SQL Server or LocalDB
- Optional: `dotnet tool install --global dotnet-ef`

## Configure SQL Server

Update `ConnectionStrings:DefaultConnection` in:

- `src/PersonalBrand.WebAPI/appsettings.Development.json` for local development
- environment variables / host settings for Production

Example LocalDB value:

```text
Server=(localdb)\mssqllocaldb;Database=PersonalBrandPlatform_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

## Run the project

```bash
dotnet restore
dotnet build
dotnet run --project src/PersonalBrand.WebAPI
```

Useful endpoints:

- Swagger: `/swagger` (Development/Staging)
- Status: `/api/v1/status`
- Health: `/health`
- Auth register: `POST /api/v1/auth/register`
- Auth login: `POST /api/v1/auth/login`
- Current user: `GET /api/v1/auth/me` (Bearer token)

## Apply EF Core migrations

```bash
dotnet ef migrations add InitialIdentity \
  --project src/PersonalBrand.Infrastructure \
  --startup-project src/PersonalBrand.WebAPI \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/PersonalBrand.Infrastructure \
  --startup-project src/PersonalBrand.WebAPI
```

## Run tests

```bash
dotnet test
```

## Hosting constraint

Production target is **Windows Shared Hosting (IIS) on Bertina**.

Docker is not required for production. Verify .NET 10 Hosting Bundle support, SQL Server availability, IIS/AspNetCoreModuleV2 configuration, writable upload/log folders, and secret configuration before deployment.

Details: [docs/deployment/BERTINA_IIS.md](docs/deployment/BERTINA_IIS.md)
