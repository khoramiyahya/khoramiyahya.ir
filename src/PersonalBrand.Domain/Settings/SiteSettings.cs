using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Settings;

/// <summary>
/// Singleton site-wide settings for the personal brand platform.
/// </summary>
public sealed class SiteSettings : AggregateRoot<Guid>
{
    public const string SingletonKey = "default";

    private SiteSettings()
    {
    }

    public string Key { get; private set; } = SingletonKey;

    public string SiteName { get; private set; } = string.Empty;

    public string? Tagline { get; private set; }

    public string? MetaTitle { get; private set; }

    public string? MetaDescription { get; private set; }

    public string? MetaKeywords { get; private set; }

    public string? CanonicalBaseUrl { get; private set; }

    public string? OgImageUrl { get; private set; }

    public string? FaviconUrl { get; private set; }

    public string? PrimaryEmail { get; private set; }

    public string? PrimaryPhone { get; private set; }

    public string? Address { get; private set; }

    public string? GoogleAnalyticsId { get; private set; }

    public bool IsMaintenanceMode { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SiteSettings CreateDefault(DateTimeOffset utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            Key = SingletonKey,
            SiteName = "Personal Brand",
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            IsMaintenanceMode = false
        };

    public void Update(
        string siteName,
        string? tagline,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
        string? canonicalBaseUrl,
        string? ogImageUrl,
        string? faviconUrl,
        string? primaryEmail,
        string? primaryPhone,
        string? address,
        string? googleAnalyticsId,
        bool isMaintenanceMode,
        DateTimeOffset utcNow)
    {
        SiteName = NormalizeRequiredText(siteName, "Site name", 200);
        Tagline = NormalizeOptionalText(tagline, "Tagline", 300);
        MetaTitle = NormalizeOptionalText(metaTitle, "Meta title", 200);
        MetaDescription = NormalizeOptionalText(metaDescription, "Meta description", 500);
        MetaKeywords = NormalizeOptionalText(metaKeywords, "Meta keywords", 500);
        CanonicalBaseUrl = NormalizeOptionalUrl(canonicalBaseUrl, "Canonical base URL");
        OgImageUrl = NormalizeOptionalUrl(ogImageUrl, "Open Graph image URL");
        FaviconUrl = NormalizeOptionalUrl(faviconUrl, "Favicon URL");
        PrimaryEmail = NormalizeOptionalEmail(primaryEmail);
        PrimaryPhone = NormalizeOptionalText(primaryPhone, "Primary phone", 50);
        Address = NormalizeOptionalText(address, "Address", 500);
        GoogleAnalyticsId = NormalizeOptionalText(googleAnalyticsId, "Google Analytics ID", 50);
        IsMaintenanceMode = isMaintenanceMode;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalUrl(string? value, string fieldName)
    {
        var normalized = NormalizeOptionalText(value, fieldName, 500);
        if (normalized is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException($"{fieldName} must be a valid http or https URL.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalEmail(string? value)
    {
        var normalized = NormalizeOptionalText(value, "Primary email", 256);
        if (normalized is null)
        {
            return null;
        }

        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
        {
            throw new DomainException("Primary email must be a valid email address.");
        }

        return normalized.ToLowerInvariant();
    }
}
