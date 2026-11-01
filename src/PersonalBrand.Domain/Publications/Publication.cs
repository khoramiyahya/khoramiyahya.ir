using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Publications;

/// <summary>
/// External publication owned by a user account.
/// </summary>
public sealed class Publication : AggregateRoot<Guid>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "article",
        "book",
        "paper",
        "podcast"
    };

    private Publication()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Publisher { get; private set; } = string.Empty;

    public string PublicationType { get; private set; } = "article";

    public string? Description { get; private set; }

    public string? Url { get; private set; }

    public DateOnly PublishedOn { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Publication Create(
        string userId,
        string title,
        string publisher,
        DateOnly publishedOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Publication owner is required.");
        }

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsFeatured = false,
            IsPublished = false
        };

        publication.ApplyContent(title, publisher, "article", null, null, publishedOn);
        return publication;
    }

    public void UpdateDetails(
        string title,
        string publisher,
        string publicationType,
        string? description,
        string? url,
        DateOnly publishedOn,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyContent(title, publisher, publicationType, description, url, publishedOn);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Publisher))
        {
            throw new DomainException("A published publication requires a title and publisher.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(
        string title,
        string publisher,
        string publicationType,
        string? description,
        string? url,
        DateOnly publishedOn)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        Publisher = NormalizeRequiredText(publisher, "Publisher", 200);
        PublicationType = NormalizeType(publicationType);
        Description = NormalizeOptionalText(description, "Description", 4000);
        Url = NormalizeOptionalUrl(url);
        PublishedOn = publishedOn;
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Publication type is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(normalized))
        {
            throw new DomainException("Publication type must be article, book, paper, or podcast.");
        }

        return normalized;
    }

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

    private static string? NormalizeOptionalUrl(string? value)
    {
        var normalized = NormalizeOptionalText(value, "URL", 500);
        if (normalized is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("URL must be a valid http or https URL.");
        }

        return normalized;
    }
}
