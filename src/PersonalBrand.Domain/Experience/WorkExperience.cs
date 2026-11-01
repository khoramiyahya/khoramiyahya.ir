using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Experience;

/// <summary>
/// Work experience entry owned by a user account.
/// </summary>
public sealed class WorkExperience : AggregateRoot<Guid>
{
    private WorkExperience()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Company { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public string? Description { get; private set; }

    public string? CompanyUrl { get; private set; }

    public DateOnly StartedOn { get; private set; }

    public DateOnly? EndedOn { get; private set; }

    public bool IsCurrent { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static WorkExperience Create(
        string userId,
        string company,
        string title,
        DateOnly startedOn,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Experience owner is required.");
        }

        var experience = new WorkExperience
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

        experience.ApplyCore(company, title, startedOn, null, false);
        return experience;
    }

    public void UpdateDetails(
        string company,
        string title,
        string? location,
        string? description,
        string? companyUrl,
        DateOnly startedOn,
        DateOnly? endedOn,
        bool isCurrent,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyCore(company, title, startedOn, endedOn, isCurrent);
        Location = NormalizeOptionalText(location, "Location", 200);
        Description = NormalizeOptionalText(description, "Description", 4000);
        CompanyUrl = NormalizeOptionalUrl(companyUrl, "Company URL");
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Company) || string.IsNullOrWhiteSpace(Title))
        {
            throw new DomainException("A published experience requires a company and title.");
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
        string company,
        string title,
        DateOnly startedOn,
        DateOnly? endedOn,
        bool isCurrent)
    {
        Company = NormalizeRequiredText(company, "Company", 200);
        Title = NormalizeRequiredText(title, "Title", 200);
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
