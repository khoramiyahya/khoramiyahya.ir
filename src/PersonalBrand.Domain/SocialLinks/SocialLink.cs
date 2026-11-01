using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.SocialLinks;

/// <summary>
/// Ordered social or external profile link owned by a user account.
/// </summary>
public sealed class SocialLink : AggregateRoot<Guid>
{
    private SocialLink()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Platform { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string? Icon { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SocialLink Create(
        string userId,
        string platform,
        string label,
        string url,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Social link owner is required.");
        }

        var link = new SocialLink
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false
        };

        link.ApplyContent(platform, label, url, null);
        return link;
    }

    public void UpdateDetails(
        string platform,
        string label,
        string url,
        string? icon,
        int displayOrder,
        DateTimeOffset utcNow)
    {
        ApplyContent(platform, label, url, icon);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Platform)
            || string.IsNullOrWhiteSpace(Label)
            || string.IsNullOrWhiteSpace(Url))
        {
            throw new DomainException("A published social link requires a platform, label, and URL.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(string platform, string label, string url, string? icon)
    {
        Platform = NormalizeRequiredText(platform, "Platform", 100);
        Label = NormalizeRequiredText(label, "Label", 100);
        Url = NormalizeRequiredUrl(url);
        Icon = NormalizeOptionalText(icon, "Icon", 100);
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

    private static string NormalizeRequiredUrl(string value)
    {
        var normalized = NormalizeRequiredText(value, "URL", 500);

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("URL must be a valid http or https URL.");
        }

        return normalized;
    }
}
