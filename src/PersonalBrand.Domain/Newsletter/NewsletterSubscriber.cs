using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Newsletter;

/// <summary>
/// Newsletter subscriber captured from the public site.
/// </summary>
public sealed class NewsletterSubscriber : AggregateRoot<Guid>
{
    private NewsletterSubscriber()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string? Name { get; private set; }

    public bool IsActive { get; private set; }

    public string? ClientIp { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset SubscribedAtUtc { get; private set; }

    public DateTimeOffset? UnsubscribedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static NewsletterSubscriber Create(
        string email,
        DateTimeOffset utcNow,
        string? name = null,
        string? clientIp = null,
        string? userAgent = null)
    {
        var subscriber = new NewsletterSubscriber
        {
            Id = Guid.NewGuid(),
            SubscribedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            IsActive = true
        };

        subscriber.ApplyIdentity(email, name);
        subscriber.ClientIp = NormalizeOptionalText(clientIp, "Client IP", 100);
        subscriber.UserAgent = NormalizeOptionalText(userAgent, "User agent", 500);
        return subscriber;
    }

    public void Resubscribe(
        DateTimeOffset utcNow,
        string? name = null,
        string? clientIp = null,
        string? userAgent = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = NormalizeOptionalText(name, "Name", 150);
        }

        ClientIp = NormalizeOptionalText(clientIp, "Client IP", 100) ?? ClientIp;
        UserAgent = NormalizeOptionalText(userAgent, "User agent", 500) ?? UserAgent;
        IsActive = true;
        UnsubscribedAtUtc = null;
        SubscribedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Unsubscribe(DateTimeOffset utcNow)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UnsubscribedAtUtc = utcNow;
        Touch(utcNow);
    }

    private void ApplyIdentity(string email, string? name)
    {
        Email = NormalizeEmail(email);
        Name = NormalizeOptionalText(name, "Name", 150);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeEmail(string email)
    {
        var normalized = NormalizeRequiredText(email, "Email", 256);
        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
        {
            throw new DomainException("Email must be a valid email address.");
        }

        return normalized.ToLowerInvariant();
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
}
