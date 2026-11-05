using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

using PersonalBrand.Application.Options;
using PersonalBrand.WebAPI.Extensions;

namespace PersonalBrand.UnitTests.Configuration;

public sealed class StartupConfigurationValidationTests
{
    [Fact]
    public void Valid_Production_Configuration_Should_Pass()
    {
        var configuration = BuildConfiguration(CreateValidValues());

        configuration.ValidateStartupConfiguration(CreateEnvironment(Environments.Production));
    }

    [Fact]
    public void Strictly_Encrypted_Production_Connection_Should_Pass()
    {
        var values = CreateValidValues();
        values["ConnectionStrings:DefaultConnection"] =
            "Server=sql.example.com;Database=PersonalBrand;Integrated Security=True;Encrypt=Strict;TrustServerCertificate=False";

        BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-connection-string")]
    [InlineData("Server=sql.example.com")]
    [InlineData("Server=(localdb)\\mssqllocaldb;Database=PersonalBrand")]
    [InlineData("Server=sql.example.com;Database=PersonalBrand;Persist Security Info=True")]
    [InlineData("Server=sql.example.com;Database=PersonalBrand;Encrypt=False")]
    [InlineData("Server=sql.example.com;Database=PersonalBrand;Encrypt=True;TrustServerCertificate=True")]
    public void Invalid_Production_Connection_Should_Fail_Without_Echoing_The_Value(string connectionString)
    {
        var values = CreateValidValues();
        values["ConnectionStrings:DefaultConnection"] = connectionString;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));

        if (!string.IsNullOrEmpty(connectionString))
        {
            Assert.DoesNotContain(connectionString, exception.Message, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("short")]
    [InlineData("TEST_ONLY_SIGNING_KEY_MUST_BE_AT_LEAST_32_CHARS")]
    [InlineData("REPLACE_WITH_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS")]
    public void Invalid_Production_Jwt_Key_Should_Fail_Without_Echoing_The_Key(string signingKey)
    {
        var values = CreateValidValues();
        values["Jwt:SigningKey"] = signingKey;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));

        Assert.DoesNotContain(signingKey, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("https://*.example.com")]
    [InlineData("https://example.com/path")]
    [InlineData(" https://example.com")]
    public void Invalid_Production_Cors_Origin_Should_Fail(string origin)
    {
        var values = CreateValidValues();
        values["Cors:AllowedOrigins:0"] = origin;

        Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));
    }

    [Fact]
    public void Missing_Production_Cors_Origin_Should_Fail()
    {
        var values = CreateValidValues();
        values.Remove("Cors:AllowedOrigins:0");

        Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));
    }

    [Fact]
    public void Duplicate_Cors_Origins_Should_Fail()
    {
        var values = CreateValidValues();
        values["Cors:AllowedOrigins:1"] = values["Cors:AllowedOrigins:0"];

        Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));
    }

    [Fact]
    public void Partial_Bootstrap_Administrator_Should_Fail()
    {
        var values = CreateValidValues();
        values["BootstrapAdmin:Email"] = "admin@example.com";

        Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production)));
    }

    [Fact]
    public void Complete_Bootstrap_Administrator_Should_Pass()
    {
        var values = CreateValidValues();
        values["BootstrapAdmin:Email"] = "admin@example.com";
        values["BootstrapAdmin:Password"] = "StrongPass1!";

        BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(Environments.Production));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void Placeholder_Bootstrap_Password_Should_Fail(string environmentName)
    {
        var values = CreateValidValues();
        values["BootstrapAdmin:Email"] = "admin@example.com";
        values["BootstrapAdmin:Password"] = "ChangeMe_Admin_123!";

        Assert.Throws<InvalidOperationException>(() =>
            BuildConfiguration(values).ValidateStartupConfiguration(CreateEnvironment(environmentName)));
    }

    [Fact]
    public void Production_Json_Should_Not_Inherit_Development_Cors_Origins()
    {
        var webApiPath = ResolveWebApiPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(webApiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Production.json", optional: false)
            .Build();

        var options = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>();

        Assert.NotNull(options);
        Assert.Empty(options.AllowedOrigins);
    }

    [Fact]
    public void Tracked_AppSettings_Should_Not_Contain_Secrets()
    {
        var webApiPath = ResolveWebApiPath();

        foreach (var filePath in Directory.EnumerateFiles(webApiPath, "appsettings*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = document.RootElement;

            if (root.TryGetProperty("ConnectionStrings", out var connectionStrings)
                && connectionStrings.TryGetProperty("DefaultConnection", out var connectionString))
            {
                var value = connectionString.GetString() ?? string.Empty;
                Assert.DoesNotContain("Password=", value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("User Id=", value, StringComparison.OrdinalIgnoreCase);
            }

            if (root.TryGetProperty("Jwt", out var jwt)
                && jwt.TryGetProperty("SigningKey", out var signingKey))
            {
                Assert.True(string.IsNullOrWhiteSpace(signingKey.GetString()), $"SigningKey must be empty in {Path.GetFileName(filePath)}.");
            }

            if (root.TryGetProperty("BootstrapAdmin", out var bootstrapAdmin)
                && bootstrapAdmin.TryGetProperty("Password", out var password))
            {
                Assert.True(string.IsNullOrWhiteSpace(password.GetString()), $"Bootstrap password must be empty in {Path.GetFileName(filePath)}.");
            }
        }
    }

    private static Dictionary<string, string?> CreateValidValues() => new()
    {
        ["ConnectionStrings:DefaultConnection"] = "Server=sql.example.com;Database=PersonalBrand;Integrated Security=True;Persist Security Info=False;Encrypt=True",
        ["Jwt:Issuer"] = "PersonalBrandPlatform",
        ["Jwt:Audience"] = "PersonalBrandPlatform.Clients",
        ["Jwt:SigningKey"] = "YH6XQdoP6YgbapM1d2W4zkv9qNEB7oU0e5t8r3c1sA4=",
        ["Jwt:AccessTokenExpirationMinutes"] = "60",
        ["Jwt:RefreshTokenExpirationDays"] = "14",
        ["Cors:AllowedOrigins:0"] = "https://example.com",
        ["BootstrapAdmin:Email"] = string.Empty,
        ["BootstrapAdmin:Password"] = string.Empty
    };

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static IHostEnvironment CreateEnvironment(string environmentName) =>
        new TestHostEnvironment { EnvironmentName = environmentName };

    private static string ResolveWebApiPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var path = Path.Combine(current.FullName, "src", "PersonalBrand.WebAPI");
            if (File.Exists(Path.Combine(path, "appsettings.json")))
            {
                return path;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate PersonalBrand.WebAPI.");
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "PersonalBrand.UnitTests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
