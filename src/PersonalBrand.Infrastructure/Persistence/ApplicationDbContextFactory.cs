using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PersonalBrand.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tools so they do not start the WebAPI host.
/// This avoids the noisy <c>HostAbortedException</c> during migrations.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = ResolveWebApiContentRoot();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found for design-time DbContext creation.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveWebApiContentRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var webApiPath = Path.Combine(current.FullName, "src", "PersonalBrand.WebAPI");
            if (File.Exists(Path.Combine(webApiPath, "appsettings.json")))
            {
                return webApiPath;
            }

            var siblingWebApiPath = Path.Combine(current.FullName, "PersonalBrand.WebAPI");
            if (File.Exists(Path.Combine(siblingWebApiPath, "appsettings.json")))
            {
                return siblingWebApiPath;
            }

            if (File.Exists(Path.Combine(current.FullName, "appsettings.json"))
                && current.Name.Equals("PersonalBrand.WebAPI", StringComparison.OrdinalIgnoreCase))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate PersonalBrand.WebAPI appsettings.json for design-time configuration.");
    }
}
