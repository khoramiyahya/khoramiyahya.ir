using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Navigation;

/// <summary>
/// Ordered site navigation / menu item owned by a user account.
/// </summary>
public sealed class NavigationItem : AggregateRoot<Guid>
{
    private NavigationItem()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public bool OpensInNewTab { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static NavigationItem Create(
        string userId,
        string label,
        string url,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Navigation item owner is required.");
        }

        var item = new NavigationItem
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false,
            OpensInNewTab = false
        };

        item.ApplyContent(label, url, "header", false);
        return item;
    }

    public void UpdateDetails(
        string label,
        string url,
        string? location,
        bool opensInNewTab,
        int displayOrder,
        DateTimeOffset utcNow)
    {
        ApplyContent(label, url, location, opensInNewTab);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Label) || string.IsNullOrWhiteSpace(Url))
        {
            throw new DomainException("A published navigation item requires a label and URL.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(string label, string url, string? location, bool opensInNewTab)
    {
        Label = NormalizeRequiredText(label, "Label", 100);
        Url = NormalizeRequiredUrlOrPath(url);
        Location = NormalizeOptionalText(location, "Location", 50) ?? "header";
        OpensInNewTab = opensInNewTab;
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

    private static string NormalizeRequiredUrlOrPath(string value)
    {
        var normalized = NormalizeRequiredText(value, "URL", 500);

        if (normalized.StartsWith('/') || normalized.StartsWith('#'))
        {
            return normalized;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("URL must be a relative path or a valid http/https URL.");
        }

        return normalized;
    }
}
