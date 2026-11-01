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
- Public services: `GET /api/v1/services/public`
- Public service: `GET /api/v1/services/public/{slug}`
- My services: `GET/POST /api/v1/services/me`
- Update/delete service: `PUT/DELETE /api/v1/services/me/{id}`
- Publish/unpublish service: `POST /api/v1/services/me/{id}/publish|unpublish`
- Public testimonials: `GET /api/v1/testimonials/public`
- Public testimonial: `GET /api/v1/testimonials/public/{id}`
- My testimonials: `GET/POST /api/v1/testimonials/me`
- Update/delete testimonial: `PUT/DELETE /api/v1/testimonials/me/{id}`
- Publish/unpublish testimonial: `POST /api/v1/testimonials/me/{id}/publish|unpublish`
- Public skills: `GET /api/v1/skills/public`
- Public skill: `GET /api/v1/skills/public/{id}`
- My skills: `GET/POST /api/v1/skills/me`
- Update/delete skill: `PUT/DELETE /api/v1/skills/me/{id}`
- Publish/unpublish skill: `POST /api/v1/skills/me/{id}/publish|unpublish`
- Public experience: `GET /api/v1/experience/public`
- Public experience item: `GET /api/v1/experience/public/{id}`
- My experience: `GET/POST /api/v1/experience/me`
- Update/delete experience: `PUT/DELETE /api/v1/experience/me/{id}`
- Publish/unpublish experience: `POST /api/v1/experience/me/{id}/publish|unpublish`
- Public education: `GET /api/v1/education/public`
- Public education item: `GET /api/v1/education/public/{id}`
- My education: `GET/POST /api/v1/education/me`
- Update/delete education: `PUT/DELETE /api/v1/education/me/{id}`
- Publish/unpublish education: `POST /api/v1/education/me/{id}/publish|unpublish`
- Public FAQ: `GET /api/v1/faq/public`
- Public FAQ item: `GET /api/v1/faq/public/{id}`
- My FAQ: `GET/POST /api/v1/faq/me`
- Update/delete FAQ: `PUT/DELETE /api/v1/faq/me/{id}`
- Publish/unpublish FAQ: `POST /api/v1/faq/me/{id}/publish|unpublish`
- Public certifications: `GET /api/v1/certifications/public`
- Public certification: `GET /api/v1/certifications/public/{id}`
- My certifications: `GET/POST /api/v1/certifications/me`
- Update/delete certification: `PUT/DELETE /api/v1/certifications/me/{id}`
- Publish/unpublish certification: `POST /api/v1/certifications/me/{id}/publish|unpublish`
- Newsletter subscribe: `POST /api/v1/newsletter/subscribe`
- Newsletter unsubscribe: `POST /api/v1/newsletter/unsubscribe`
- Newsletter subscribers: `GET /api/v1/newsletter/subscribers?activeOnly=true`
- Delete newsletter subscriber: `DELETE /api/v1/newsletter/subscribers/{id}`
- Public site settings: `GET /api/v1/settings/public`
- My site settings: `GET/PUT /api/v1/settings/me`
- Dashboard summary: `GET /api/v1/dashboard/summary`
- My media: `GET/POST /api/v1/media/me`
- Media item: `GET/PUT/DELETE /api/v1/media/me/{id}`
- Upload media: `POST /api/v1/media/me` (multipart `file`, optional `title`, `altText`)
- Public pages: `GET /api/v1/pages/public`
- Public page: `GET /api/v1/pages/public/{slug}`
- My pages: `GET/POST /api/v1/pages/me`
- Update/delete page: `PUT/DELETE /api/v1/pages/me/{id}`
- Publish/unpublish page: `POST /api/v1/pages/me/{id}/publish|unpublish`
- Public social links: `GET /api/v1/sociallinks/public`
- Public social link: `GET /api/v1/sociallinks/public/{id}`
- My social links: `GET/POST /api/v1/sociallinks/me`
- Update/delete social link: `PUT/DELETE /api/v1/sociallinks/me/{id}`
- Publish/unpublish social link: `POST /api/v1/sociallinks/me/{id}/publish|unpublish`
- Public navigation: `GET /api/v1/navigation/public?location=header`
- Public navigation item: `GET /api/v1/navigation/public/{id}`
- My navigation: `GET/POST /api/v1/navigation/me`
- Update/delete navigation: `PUT/DELETE /api/v1/navigation/me/{id}`
- Publish/unpublish navigation: `POST /api/v1/navigation/me/{id}/publish|unpublish`
- Public redirects: `GET /api/v1/redirects/public`
- Resolve redirect: `GET /api/v1/redirects/public/resolve?fromPath=/old-path`
- My redirects: `GET/POST /api/v1/redirects/me`
- Update/delete redirect: `PUT/DELETE /api/v1/redirects/me/{id}`
- Publish/unpublish redirect: `POST /api/v1/redirects/me/{id}/publish|unpublish`
- Site search: `GET /api/v1/search?q=keyword&contentType=blog&limit=20`
- Sitemap JSON: `GET /api/v1/sitemap`
- Sitemap XML: `GET /api/v1/sitemap/xml`
- Public robots.txt: `GET /api/v1/robots/public`
- My robots rules: `GET/POST /api/v1/robots/me`
- Update/delete robots rule: `PUT/DELETE /api/v1/robots/me/{id}`
- Publish/unpublish robots rule: `POST /api/v1/robots/me/{id}/publish|unpublish`
- Public partners: `GET /api/v1/partners/public`
- Public partner: `GET /api/v1/partners/public/{id}`
- My partners: `GET/POST /api/v1/partners/me`
- Update/delete partner: `PUT/DELETE /api/v1/partners/me/{id}`
- Publish/unpublish partner: `POST /api/v1/partners/me/{id}/publish|unpublish`
- Public awards: `GET /api/v1/awards/public`
- Public award: `GET /api/v1/awards/public/{id}`
- My awards: `GET/POST /api/v1/awards/me`
- Update/delete award: `PUT/DELETE /api/v1/awards/me/{id}`
- Publish/unpublish award: `POST /api/v1/awards/me/{id}/publish|unpublish`
- Public talks: `GET /api/v1/talks/public`
- Public talk: `GET /api/v1/talks/public/{id}`
- My talks: `GET/POST /api/v1/talks/me`
- Update/delete talk: `PUT/DELETE /api/v1/talks/me/{id}`
- Publish/unpublish talk: `POST /api/v1/talks/me/{id}/publish|unpublish`
- Public publications: `GET /api/v1/publications/public`
- Public publication: `GET /api/v1/publications/public/{id}`
- My publications: `GET/POST /api/v1/publications/me`
- Update/delete publication: `PUT/DELETE /api/v1/publications/me/{id}`
- Publish/unpublish publication: `POST /api/v1/publications/me/{id}/publish|unpublish`


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
