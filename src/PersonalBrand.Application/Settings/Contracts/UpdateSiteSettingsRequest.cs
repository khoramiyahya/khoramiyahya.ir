namespace PersonalBrand.Application.Settings.Contracts;

/// <summary>
/// Request payload for updating site settings.
/// </summary>
public sealed class UpdateSiteSettingsRequest
{
    public string SiteName { get; set; } = string.Empty;

    public string? Tagline { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? CanonicalBaseUrl { get; set; }

    public string? OgImageUrl { get; set; }

    public string? FaviconUrl { get; set; }

    public string? PrimaryEmail { get; set; }

    public string? PrimaryPhone { get; set; }

    public string? Address { get; set; }

    public string? GoogleAnalyticsId { get; set; }

    public bool IsMaintenanceMode { get; set; }
}
