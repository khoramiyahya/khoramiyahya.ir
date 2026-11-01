using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Talks;

/// <summary>
/// Speaking engagement owned by a user account.
/// </summary>
public sealed class Talk : AggregateRoot<Guid>
{
    private Talk()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string EventName { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public string? Description { get; private set; }

    public string? Url { get; private set; }

    public DateOnly SpokenOn { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Talk Create(
        string userId,
        string title,
        string eventName,
        DateOnly spokenOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Talk owner is required.");
        }

        var talk = new Talk
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsFeatured = false,
            IsPublished = false
        };

        talk.ApplyContent(title, eventName, null, null, null, spokenOn);
        return talk;
    }

    public void UpdateDetails(
        string title,
        string eventName,
        string? location,
        string? description,
        string? url,
        DateOnly spokenOn,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyContent(title, eventName, location, description, url, spokenOn);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(EventName))
        {
            throw new DomainException("A published talk requires a title and event name.");
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
        string eventName,
        string? location,
        string? description,
        string? url,
        DateOnly spokenOn)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        EventName = NormalizeRequiredText(eventName, "Event name", 200);
        Location = NormalizeOptionalText(location, "Location", 200);
        Description = NormalizeOptionalText(description, "Description", 4000);
        Url = NormalizeOptionalUrl(url);
        SpokenOn = spokenOn;
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
