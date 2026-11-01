namespace PersonalBrand.Application.Settings.Contracts;

/// <summary>
/// Public and owner representation of site settings.
/// </summary>
public sealed class SiteSettingsDto
{
    public required string SiteName { get; init; }

    public string? Tagline { get; init; }

    public string? MetaTitle { get; init; }

    public string? MetaDescription { get; init; }

    public string? MetaKeywords { get; init; }

    public string? CanonicalBaseUrl { get; init; }

    public string? OgImageUrl { get; init; }

    public string? FaviconUrl { get; init; }

    public string? PrimaryEmail { get; init; }

    public string? PrimaryPhone { get; init; }

    public string? Address { get; init; }

    public string? GoogleAnalyticsId { get; init; }

    public required bool IsMaintenanceMode { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
