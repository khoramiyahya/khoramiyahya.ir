using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Contact;

/// <summary>
/// Contact-form inquiry stored for the brand owner inbox.
/// </summary>
public sealed class ContactMessage : AggregateRoot<Guid>
{
    private ContactMessage()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? ClientIp { get; private set; }

    public string? UserAgent { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ContactMessage Create(
        string name,
        string email,
        string subject,
        string body,
        DateTimeOffset utcNow,
        string? phone = null,
        string? clientIp = null,
        string? userAgent = null)
    {
        var message = new ContactMessage
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = utcNow,
            IsRead = false
        };

        message.ApplyContent(name, email, subject, body, phone);
        message.ClientIp = NormalizeOptionalText(clientIp, "Client IP", 100);
        message.UserAgent = NormalizeOptionalText(userAgent, "User agent", 500);
        return message;
    }

    public void MarkRead(DateTimeOffset utcNow)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = utcNow;
    }

    public void MarkUnread()
    {
        IsRead = false;
        ReadAtUtc = null;
    }

    private void ApplyContent(
        string name,
        string email,
        string subject,
        string body,
        string? phone)
    {
        Name = NormalizeRequiredText(name, "Name", 150);
        Email = NormalizeEmail(email);
        Subject = NormalizeRequiredText(subject, "Subject", 200);
        Body = NormalizeRequiredText(body, "Message", 5000);
        Phone = NormalizeOptionalText(phone, "Phone", 50);
    }

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
