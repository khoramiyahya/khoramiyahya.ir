# Architecture

## Overview

The backend follows Clean Architecture with Modular Monolith principles.

```text
WebAPI ──► Application ──► Domain
   │            ▲
   └────► Infrastructure ──┘
```

Dependencies always point inward. Outer layers implement abstractions defined by inner layers.

## Layer responsibilities

### Domain (`PersonalBrand.Domain`)

- Entities, aggregate roots, value objects
- Domain events and domain exceptions
- Business invariants that do not depend on infrastructure

Domain has no package dependencies on Application, Infrastructure, or WebAPI.

### Application (`PersonalBrand.Application`)

- Use cases and application services
- DTOs, validation (FluentValidation), options bindings
- Application abstractions such as `IFileStorage`, `IAuditLogger`, `ICurrentUserService`

Application depends only on Domain.

### Infrastructure (`PersonalBrand.Infrastructure`)

- EF Core `ApplicationDbContext`
- SQL Server configuration and migrations
- ASP.NET Core Identity persistence
- File storage and other external integrations

Infrastructure implements Application abstractions and may reference Domain for persistence mapping.

### WebAPI (`PersonalBrand.WebAPI`)

- HTTP endpoints and middleware
- Authentication/authorization composition
- Swagger / OpenAPI
- CORS, rate limiting, health checks
- Composition root for Dependency Injection

WebAPI must not contain business rules.

## Request lifecycle

1. HTTP request enters ASP.NET Core pipeline.
2. Exception middleware wraps the request.
3. Serilog records request information.
4. CORS, rate limiting, authentication, and authorization execute.
5. Controller delegates to Application services/use cases.
6. Application orchestrates Domain rules and Infrastructure adapters.
7. Response is returned through a consistent API envelope when controllers choose to use it.

## Database access strategy

- EF Core 10 is the only persistence ORM.
- Schema evolves exclusively through EF Core migrations.
- `ApplicationDbContext` currently hosts Identity entities and is ready for future business modules.
- No generic repository is introduced by default. Prefer explicit application services and DbContext usage until a repository boundary becomes justified by a module.

## Authentication architecture

- ASP.NET Core Identity stores users/roles and hashes passwords.
- JWT Bearer authentication protects API endpoints.
- Role-based authorization is available through Identity roles (`Administrator`, `Editor`, `Viewer`).
- JWT settings are loaded from configuration and must be supplied securely in Production.
- Controllers remain free of password hashing or token cryptography details.

## Hosting notes

The production target is Windows Shared Hosting (IIS) on Bertina.

Required verification before go-live:

- .NET 10 Hosting Bundle / runtime support on the server
- SQL Server availability and compatible version
- IIS site bindings, application pool (.NET CLR: No Managed Code), and AspNetCoreModuleV2
- Writable folder for uploads (`App_Data/uploads`) and logs
- Environment variables or host panel settings for connection string and JWT secrets
- Upload size limits enforced by IIS / host panel

Docker is optional for local workflows and is not a production dependency.
