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
- Auth refresh: `POST /api/v1/auth/refresh`
- Auth revoke: `POST /api/v1/auth/revoke`
- Current user: `GET /api/v1/auth/me` (Bearer token)
- My profile: `GET/PUT /api/v1/profiles/me` (Bearer token)
- Upload avatar: `POST /api/v1/profiles/me/avatar` (multipart `file`)
- Remove avatar: `DELETE /api/v1/profiles/me/avatar`
- Publish profile: `POST /api/v1/profiles/me/publish`
- Public profile: `GET /api/v1/profiles/public/{slug}`
- Avatar files: `/uploads/{relative-path}`
- Public projects: `GET /api/v1/projects/public`
- Public project: `GET /api/v1/projects/public/{slug}`
- My projects: `GET/POST /api/v1/projects/me`
- Update/delete project: `PUT/DELETE /api/v1/projects/me/{id}`
- Upload project cover: `POST /api/v1/projects/me/{id}/cover` (multipart `file`)
- Remove project cover: `DELETE /api/v1/projects/me/{id}/cover`
- Publish/unpublish project: `POST /api/v1/projects/me/{id}/publish|unpublish`
- Public blog posts: `GET /api/v1/blog/public`
- Public blog post: `GET /api/v1/blog/public/{slug}`
- My blog posts: `GET/POST /api/v1/blog/me`
- Update/delete blog post: `PUT/DELETE /api/v1/blog/me/{id}`
- Upload blog cover: `POST /api/v1/blog/me/{id}/cover` (multipart `file`)
- Remove blog cover: `DELETE /api/v1/blog/me/{id}/cover`
- Publish/unpublish blog post: `POST /api/v1/blog/me/{id}/publish|unpublish`
- Submit contact form: `POST /api/v1/contact`
- Contact inbox: `GET /api/v1/contact/inbox?unreadOnly=false`
- Contact message: `GET /api/v1/contact/inbox/{id}`
- Mark contact read/unread: `POST /api/v1/contact/inbox/{id}/read|unread`
- Delete contact message: `DELETE /api/v1/contact/inbox/{id}`


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

Production target is **Windows Shared Hosting (IIS)**.

Docker is not required for production. Verify .NET 10 Hosting Bundle support, SQL Server availability, IIS/AspNetCoreModuleV2 configuration, writable upload/log folders, and secret configuration before deployment.

Details: [docs/deployment/IIS_SHARED_HOSTING.md](docs/deployment/IIS_SHARED_HOSTING.md)
