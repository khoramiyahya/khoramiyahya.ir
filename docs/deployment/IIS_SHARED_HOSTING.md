# Windows Shared Hosting (IIS)

## Must verify with the host

1. **.NET 10 Hosting Bundle** is installed and supported for the account.
2. **SQL Server** database is provisioned; note version, collation, and remote connection details.
3. Application pool is configured for **No Managed Code** with Integrated pipeline.
4. Site uses **AspNetCoreModuleV2**.
5. Writable paths exist for logs, uploads, and the upload directory's sibling transaction-staging directory.
6. Secrets can be supplied through environment variables or host control-panel settings without committing secrets to git.
7. Request size limits for uploads are sufficient for planned media sizes.

## Publish

```bash
dotnet publish src/PersonalBrand.WebAPI -c Release -o ./publish
```

Upload the publish output to the hosting directory and ensure `web.config` is present.

## Production configuration checklist

- Set `ASPNETCORE_ENVIRONMENT=Production`
- Set `ConnectionStrings__DefaultConnection` with encrypted SQL transport and certificate validation enabled
- Set `Jwt__SigningKey` to a cryptographically random value with at least 256 bits of entropy
- Set `Cors__AllowedOrigins__0` and subsequent indexes to the exact React frontend origin(s)
- Keep optional `BootstrapAdmin__Email` and `BootstrapAdmin__Password` values in host settings only, then remove them after successful creation
- Apply EF migrations against the production database before first traffic
- Confirm `/health` is reachable from an allowed monitoring path

If the provider only supports configuration through `web.config`, inject secrets into the published artifact or edit the server-side copy after upload. Never put real values in the tracked `src/PersonalBrand.WebAPI/web.config`.

## Compromised SQL credential rotation

The SQL credential previously committed to this repository must be treated as compromised. Rotate it through Bertina's control panel or support process:

1. Generate the replacement in the hosting control panel or an approved password manager.
2. Prefer a new least-privilege runtime login when the provider permits it; keep schema-migration permissions on a separate deployment principal.
3. Store the replacement connection string only as `ConnectionStrings__DefaultConnection` in host configuration.
4. Recycle every application-pool instance so old pooled SQL sessions cannot mask a failed rotation.
5. Verify `/health` and one authenticated database operation with the replacement credential.
6. Disable or reset the old login, terminate remaining old sessions, and verify the old credential can no longer authenticate.
7. Remove temporary migration and bootstrap-administrator credentials after use.

Do not pass passwords as command-line arguments or print connection strings in logs, CI output, tickets, or documentation. Rewriting Git history may reduce future exposure but does not replace credential rotation.
