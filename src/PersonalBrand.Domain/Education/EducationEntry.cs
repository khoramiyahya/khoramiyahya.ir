using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Education;

/// <summary>
/// Education entry owned by a user account.
/// </summary>
public sealed class EducationEntry : AggregateRoot<Guid>
{
    private EducationEntry()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Institution { get; private set; } = string.Empty;

    public string Degree { get; private set; } = string.Empty;

    public string? FieldOfStudy { get; private set; }

    public string? Location { get; private set; }

    public string? Description { get; private set; }

    public string? InstitutionUrl { get; private set; }

    public DateOnly StartedOn { get; private set; }

    public DateOnly? EndedOn { get; private set; }

    public bool IsCurrent { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static EducationEntry Create(
        string userId,
        string institution,
        string degree,
        DateOnly startedOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Education owner is required.");
        }

        var entry = new EducationEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false,
            IsFeatured = false,
            IsCurrent = false
        };

        entry.ApplyCore(institution, degree, startedOn, null, false);
        return entry;
    }

    public void UpdateDetails(
        string institution,
        string degree,
        string? fieldOfStudy,
        string? location,
        string? description,
        string? institutionUrl,
        DateOnly startedOn,
        DateOnly? endedOn,
        bool isCurrent,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyCore(institution, degree, startedOn, endedOn, isCurrent);
        FieldOfStudy = NormalizeOptionalText(fieldOfStudy, "Field of study", 200);
        Location = NormalizeOptionalText(location, "Location", 200);
        Description = NormalizeOptionalText(description, "Description", 4000);
        InstitutionUrl = NormalizeOptionalUrl(institutionUrl, "Institution URL");
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Institution) || string.IsNullOrWhiteSpace(Degree))
        {
            throw new DomainException("A published education entry requires an institution and degree.");
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
        string institution,
        string degree,
        DateOnly startedOn,
        DateOnly? endedOn,
        bool isCurrent)
    {
        Institution = NormalizeRequiredText(institution, "Institution", 200);
        Degree = NormalizeRequiredText(degree, "Degree", 200);
        StartedOn = startedOn;
        IsCurrent = isCurrent;

        if (isCurrent)
        {
            EndedOn = null;
            return;
        }

        if (endedOn is not null && endedOn < startedOn)
        {
            throw new DomainException("End date cannot be earlier than start date.");
        }

        EndedOn = endedOn;
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
