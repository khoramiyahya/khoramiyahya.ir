namespace PersonalBrand.Application.Options;

/// <summary>
/// JWT bearer token configuration bound from application settings.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key. Must be provided via secure configuration in production.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
