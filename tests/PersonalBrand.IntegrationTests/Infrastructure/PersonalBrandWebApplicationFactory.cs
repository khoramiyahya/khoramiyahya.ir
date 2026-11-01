using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonalBrand.IntegrationTests.Infrastructure;

public sealed class PersonalBrandWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"PersonalBrandPlatform_Test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        builder.UseSetting("Jwt:Issuer", "PersonalBrandPlatform.Test");
        builder.UseSetting("Jwt:Audience", "PersonalBrandPlatform.Clients.Test");
        builder.UseSetting("Jwt:SigningKey", "TEST_ONLY_SIGNING_KEY_MUST_BE_AT_LEAST_32_CHARS");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "30");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:5173");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("BootstrapAdmin:Email", string.Empty);
        builder.UseSetting("BootstrapAdmin:Password", string.Empty);
    }
}
