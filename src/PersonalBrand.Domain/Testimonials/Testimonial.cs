using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Testimonials;

/// <summary>
/// Client or peer testimonial owned by a user account.
/// </summary>
public sealed class Testimonial : AggregateRoot<Guid>
{
    private Testimonial()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string AuthorName { get; private set; } = string.Empty;

    public string? AuthorRole { get; private set; }

    public string? Company { get; private set; }

    public string Quote { get; private set; } = string.Empty;

    public int? Rating { get; private set; }

    public string? AuthorImageUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Testimonial Create(
        string userId,
        string authorName,
        string quote,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Testimonial owner is required.");
        }

        var testimonial = new Testimonial
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false,
            IsFeatured = false
        };

        testimonial.ApplyContent(authorName, quote, null, null, null, null);
        return testimonial;
    }

    public void UpdateDetails(
        string authorName,
        string quote,
        string? authorRole,
        string? company,
        int? rating,
        string? authorImageUrl,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyContent(authorName, quote, authorRole, company, rating, authorImageUrl);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(AuthorName) || string.IsNullOrWhiteSpace(Quote))
        {
            throw new DomainException("A published testimonial requires an author name and quote.");
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
        string authorName,
        string quote,
        string? authorRole,
        string? company,
        int? rating,
        string? authorImageUrl)
    {
        AuthorName = NormalizeRequiredText(authorName, "Author name", 150);
        Quote = NormalizeRequiredText(quote, "Quote", 2000);
        AuthorRole = NormalizeOptionalText(authorRole, "Author role", 150);
        Company = NormalizeOptionalText(company, "Company", 150);
        AuthorImageUrl = NormalizeOptionalUrl(authorImageUrl, "Author image URL");
        Rating = NormalizeRating(rating);
    }

    private static int? NormalizeRating(int? rating)
    {
        if (rating is null)
        {
            return null;
        }

        if (rating is < 1 or > 5)
        {
            throw new DomainException("Rating must be between 1 and 5.");
        }

        return rating;
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
}
