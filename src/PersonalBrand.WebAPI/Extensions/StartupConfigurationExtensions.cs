using System.Net.Mail;
using System.Text;

using Microsoft.Data.SqlClient;

using PersonalBrand.Application.Options;
using PersonalBrand.Infrastructure.Options;

namespace PersonalBrand.WebAPI.Extensions;

internal static class StartupConfigurationExtensions
{
    private const int MinimumSigningKeyBytes = 32;
    private const int MaximumAccessTokenMinutes = 1440;
    private const int MaximumRefreshTokenDays = 365;

    public static void ValidateStartupConfiguration(
        this IConfiguration configuration,
        IHostEnvironment environment)
    {
        ValidateConnectionString(configuration, environment);
        ValidateJwt(configuration, environment);
        ValidateCors(configuration, environment);
        ValidateBootstrapAdministrator(configuration);
    }

    private static void ValidateConnectionString(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be configured through secure configuration.");
        }

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be a valid SQL Server connection string.");
        }

        if (string.IsNullOrWhiteSpace(builder.DataSource)
            || string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must specify a server and database.");
        }

        if (environment.IsProduction()
            && (builder.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(builder.AttachDBFilename)
                || builder.PersistSecurityInfo
                || builder.Encrypt == SqlConnectionEncryptOption.Optional
                || builder.TrustServerCertificate))
        {
            throw new InvalidOperationException(
                "Production SQL configuration must use authenticated encryption and cannot use LocalDB, an attached file, or Persist Security Info.");
        }
    }

    private static void ValidateJwt(IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is required.");

        if (string.IsNullOrWhiteSpace(options.Issuer)
            || string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience are required.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey)
            || Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be a cryptographically random secret of at least 256 bits.");
        }

        if (environment.IsProduction() && LooksLikePlaceholder(options.SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey cannot use a development or test placeholder in Production.");
        }

        if (options.AccessTokenExpirationMinutes is < 1 or > MaximumAccessTokenMinutes
            || options.RefreshTokenExpirationDays is < 1 or > MaximumRefreshTokenDays)
        {
            throw new InvalidOperationException(
                $"JWT lifetimes must be 1-{MaximumAccessTokenMinutes} minutes for access tokens and 1-{MaximumRefreshTokenDays} days for refresh tokens.");
        }
    }

    private static void ValidateCors(IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        var origins = options.AllowedOrigins ?? [];

        if (!environment.IsDevelopment() && origins.Length == 0)
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must contain at least one explicit origin outside Development.");
        }

        var uniqueOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var origin in origins)
        {
            if (string.IsNullOrWhiteSpace(origin)
                || !string.Equals(origin, origin.Trim(), StringComparison.Ordinal)
                || origin.Contains('*')
                || !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(uri.UserInfo)
                || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment)
                || !string.Equals(origin, uri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Each Cors:AllowedOrigins value must be a unique HTTP or HTTPS authority without wildcards, credentials, paths, queries, fragments, or surrounding whitespace.");
            }

            if (environment.IsProduction() && uri.IsLoopback)
            {
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins cannot contain a loopback origin in Production.");
            }

            if (!uniqueOrigins.Add(origin))
            {
                throw new InvalidOperationException("Cors:AllowedOrigins cannot contain duplicates.");
            }
        }
    }

    private static void ValidateBootstrapAdministrator(IConfiguration configuration)
    {
        var options = configuration.GetSection(BootstrapAdminOptions.SectionName).Get<BootstrapAdminOptions>()
            ?? new BootstrapAdminOptions();
        var hasEmail = !string.IsNullOrWhiteSpace(options.Email);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        if (!hasEmail && !hasPassword)
        {
            return;
        }

        if (!hasEmail || !hasPassword)
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Email and BootstrapAdmin:Password must both be supplied or both be empty.");
        }

        if (!MailAddress.TryCreate(options.Email.Trim(), out var address)
            || !string.Equals(address.Address, options.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("BootstrapAdmin:Email must be a valid email address.");
        }

        if (options.Password.Length < 8
            || !options.Password.Any(char.IsUpper)
            || !options.Password.Any(char.IsLower)
            || !options.Password.Any(char.IsDigit)
            || options.Password.All(char.IsLetterOrDigit))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Password does not satisfy the configured Identity password policy.");
        }

        if (LooksLikePlaceholder(options.Password))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Password cannot use a development or example placeholder.");
        }
    }

    private static bool LooksLikePlaceholder(string value)
    {
        var normalized = string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();

        return normalized.Contains("CHANGEME", StringComparison.Ordinal)
            || normalized.Contains("REPLACE", StringComparison.Ordinal)
            || normalized.Contains("TESTONLY", StringComparison.Ordinal)
            || normalized.Contains("PLACEHOLDER", StringComparison.Ordinal)
            || value.Contains('<')
            || value.Contains('>');
    }
}
