using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;

namespace PersonalBrand.Domain.Services;

/// <summary>
/// Service offering aggregate owned by a user account.
/// </summary>
public sealed class ServiceOffer : AggregateRoot<Guid>
{
    private ServiceOffer()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string? Description { get; private set; }

    public string? PriceLabel { get; private set; }

    public string? IconName { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ServiceOffer Create(
        string userId,
        string title,
        DateTimeOffset utcNow,
        string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Service owner is required.");
        }

        var normalizedTitle = NormalizeRequiredText(title, "Title", 200);
        return new ServiceOffer
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
            IsPublished = false,
            IsFeatured = false
        };
    }

    public void UpdateDetails(
        string title,
        string? summary,
        string? description,
        string? priceLabel,
        string? iconName,
        int displayOrder,
        bool isFeatured,
        string? slug,
        DateTimeOffset utcNow)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        Summary = NormalizeOptionalText(summary, "Summary", 500);
        Description = NormalizeOptionalText(description, "Description", 8000);
        PriceLabel = NormalizeOptionalText(priceLabel, "Price label", 100);
        IconName = NormalizeOptionalText(iconName, "Icon name", 100);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;

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
            throw new DomainException("A published service requires a title and slug.");
        }

        IsPublished = true;
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
