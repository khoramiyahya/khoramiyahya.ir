using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Certifications;

/// <summary>
/// Professional certification owned by a user account.
/// </summary>
public sealed class Certification : AggregateRoot<Guid>
{
    private Certification()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Issuer { get; private set; } = string.Empty;

    public string? CredentialId { get; private set; }

    public string? CredentialUrl { get; private set; }

    public string? Description { get; private set; }

    public DateOnly IssuedOn { get; private set; }

    public DateOnly? ExpiresOn { get; private set; }

    public bool DoesNotExpire { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Certification Create(
        string userId,
        string name,
        string issuer,
        DateOnly issuedOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Certification owner is required.");
        }

        var certification = new Certification
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false,
            IsFeatured = false,
            DoesNotExpire = true
        };

        certification.ApplyCore(name, issuer, issuedOn, null, true);
        return certification;
    }

    public void UpdateDetails(
        string name,
        string issuer,
        string? credentialId,
        string? credentialUrl,
        string? description,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        bool doesNotExpire,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyCore(name, issuer, issuedOn, expiresOn, doesNotExpire);
        CredentialId = NormalizeOptionalText(credentialId, "Credential ID", 200);
        CredentialUrl = NormalizeOptionalUrl(credentialUrl, "Credential URL");
        Description = NormalizeOptionalText(description, "Description", 2000);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Issuer))
        {
            throw new DomainException("A published certification requires a name and issuer.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyCore(
        string name,
        string issuer,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        bool doesNotExpire)
    {
        Name = NormalizeRequiredText(name, "Name", 200);
        Issuer = NormalizeRequiredText(issuer, "Issuer", 200);
        IssuedOn = issuedOn;
        DoesNotExpire = doesNotExpire;

        if (doesNotExpire)
        {
            ExpiresOn = null;
            return;
        }

        if (expiresOn is null)
        {
            throw new DomainException("Expiration date is required when the certification expires.");
        }

        if (expiresOn < issuedOn)
        {
            throw new DomainException("Expiration date cannot be earlier than issue date.");
        }

        ExpiresOn = expiresOn;
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
