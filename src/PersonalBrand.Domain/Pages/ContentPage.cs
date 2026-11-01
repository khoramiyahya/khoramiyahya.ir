using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;

namespace PersonalBrand.Domain.Pages;

/// <summary>
/// CMS-style content page owned by a user account.
/// </summary>
public sealed class ContentPage : AggregateRoot<Guid>
{
    private ContentPage()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string? Body { get; private set; }

    public string? MetaTitle { get; private set; }

    public string? MetaDescription { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ContentPage Create(
        string userId,
        string title,
        DateTimeOffset utcNow,
        string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Page owner is required.");
        }

        var normalizedTitle = NormalizeRequiredText(title, "Title", 200);
        return new ContentPage
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            Title = normalizedTitle,
            Slug = string.IsNullOrWhiteSpace(slug)
                ? ProfileSlug.FromDisplayName(normalizedTitle)
                : ProfileSlug.Normalize(slug),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false
        };
    }

    public void UpdateDetails(
        string title,
        string? summary,
        string? body,
        string? metaTitle,
        string? metaDescription,
        int displayOrder,
        string? slug,
        DateTimeOffset utcNow)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        Summary = NormalizeOptionalText(summary, "Summary", 500);
        Body = NormalizeOptionalText(body, "Body", 50000);
        MetaTitle = NormalizeOptionalText(metaTitle, "Meta title", 200);
        MetaDescription = NormalizeOptionalText(metaDescription, "Meta description", 500);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;

        if (!string.IsNullOrWhiteSpace(slug))
        {
            EnsureSlug(slug, utcNow);
            return;
        }

        Touch(utcNow);
    }

    public void EnsureSlug(string slug, DateTimeOffset utcNow)
    {
        Slug = ProfileSlug.Normalize(slug);
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Slug))
        {
            throw new DomainException("A published page requires a title and slug.");
        }

        if (string.IsNullOrWhiteSpace(Body))
        {
            throw new DomainException("A published page requires a body.");
        }

        IsPublished = true;
        PublishedAtUtc ??= utcNow;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
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
}
