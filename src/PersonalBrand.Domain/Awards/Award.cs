using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Awards;

/// <summary>
/// Honor or award owned by a user account.
/// </summary>
public sealed class Award : AggregateRoot<Guid>
{
    private Award()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Issuer { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Url { get; private set; }

    public DateOnly AwardedOn { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Award Create(
        string userId,
        string title,
        string issuer,
        DateOnly awardedOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Award owner is required.");
        }

        var award = new Award
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsFeatured = false,
            IsPublished = false
        };

        award.ApplyContent(title, issuer, null, null, awardedOn);
        return award;
    }

    public void UpdateDetails(
        string title,
        string issuer,
        string? description,
        string? url,
        DateOnly awardedOn,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyContent(title, issuer, description, url, awardedOn);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Issuer))
        {
            throw new DomainException("A published award requires a title and issuer.");
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
        string issuer,
        string? description,
        string? url,
        DateOnly awardedOn)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        Issuer = NormalizeRequiredText(issuer, "Issuer", 200);
        Description = NormalizeOptionalText(description, "Description", 2000);
        Url = NormalizeOptionalUrl(url);
        AwardedOn = awardedOn;
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
