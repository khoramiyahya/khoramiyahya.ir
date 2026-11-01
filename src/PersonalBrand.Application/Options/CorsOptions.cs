namespace PersonalBrand.Application.Options;

/// <summary>
/// CORS configuration for the React frontend.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
