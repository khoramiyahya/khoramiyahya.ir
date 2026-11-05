# Database

This folder stores database-related artifacts that are useful outside of EF Core migrations.

## Strategy

- Schema changes are owned by Entity Framework Core migrations in `src/PersonalBrand.Infrastructure`.
- Do not hand-edit production schema outside the migration workflow unless there is an emergency hotfix that is immediately followed by a matching migration.
- Development can use LocalDB or a dedicated SQL Server instance.
- Production on shared hosting must use the SQL Server database provisioned by the host.

## Useful commands

From the repository root:

```bash
dotnet tool restore
dotnet ef migrations add <MigrationName> --project src/PersonalBrand.Infrastructure --startup-project src/PersonalBrand.WebAPI --output-dir Persistence/Migrations
dotnet ef database update --project src/PersonalBrand.Infrastructure --startup-project src/PersonalBrand.WebAPI
```

Verify that the EF model matches the committed migrations:

```bash
dotnet ef migrations has-pending-model-changes --project src/PersonalBrand.Infrastructure --startup-project src/PersonalBrand.WebAPI
```

The repository pins `dotnet-ef` in `.config/dotnet-tools.json`; do not rely on a machine-global version.
