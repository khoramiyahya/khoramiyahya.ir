# Bertina Windows Shared Hosting (IIS)

## Must verify with the host

1. **.NET 10 Hosting Bundle** is installed and supported for the account.
2. **SQL Server** database is provisioned; note version, collation, and remote connection details.
3. Application pool is configured for **No Managed Code** with Integrated pipeline.
4. Site uses **AspNetCoreModuleV2**.
5. A writable path exists for uploads and logs.
6. Secrets can be supplied through environment variables or `web.config`/`appsettings` overrides without committing secrets to git.
7. Request size limits for uploads are sufficient for planned media sizes.

## Publish

```bash
dotnet publish src/PersonalBrand.WebAPI -c Release -o ./publish
```

Upload the publish output to the hosting directory and ensure `web.config` is present.

## Production configuration checklist

- Set `ASPNETCORE_ENVIRONMENT=Production`
- Set SQL connection string
- Set a strong `Jwt:SigningKey` (32+ characters)
- Restrict `Cors:AllowedOrigins` to the React frontend origin(s)
- Apply EF migrations against the production database before first traffic
- Confirm `/health` is reachable from an allowed monitoring path
