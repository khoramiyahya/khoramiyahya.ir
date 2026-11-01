using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;

namespace PersonalBrand.Domain.Projects;

/// <summary>
/// Portfolio project aggregate owned by a user account.
/// </summary>
public sealed class PortfolioProject : AggregateRoot<Guid>
{
    private PortfolioProject()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string? Description { get; private set; }

    public string? CoverImagePath { get; private set; }

    public string? ProjectUrl { get; private set; }

    public string? RepoUrl { get; private set; }

    public string? Technologies { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateOnly? StartedOn { get; private set; }

    public DateOnly? CompletedOn { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static PortfolioProject Create(
        string userId,
        string title,
        DateTimeOffset utcNow,
        string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Project owner is required.");
        }

        var normalizedTitle = NormalizeRequiredText(title, "Title", 200);
        var project = new PortfolioProject
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

        return project;
    }

    public void UpdateDetails(
        string title,
        string? summary,
        string? description,
        string? projectUrl,
        string? repoUrl,
        string? technologies,
        int displayOrder,
        bool isFeatured,
        DateOnly? startedOn,
        DateOnly? completedOn,
        string? slug,
        DateTimeOffset utcNow)
    {
        Title = NormalizeRequiredText(title, "Title", 200);
        Summary = NormalizeOptionalText(summary, "Summary", 500);
        Description = NormalizeOptionalText(description, "Description", 8000);
        ProjectUrl = NormalizeOptionalUrl(projectUrl, "Project URL");
        RepoUrl = NormalizeOptionalUrl(repoUrl, "Repository URL");
        Technologies = NormalizeOptionalText(technologies, "Technologies", 500);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        StartedOn = startedOn;
        CompletedOn = completedOn;

        if (StartedOn is not null && CompletedOn is not null && CompletedOn < StartedOn)
        {
            throw new DomainException("Completed date cannot be earlier than started date.");
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            EnsureSlug(slug, utcNow);
            return;
        }

        Touch(utcNow);
    }

    public void SetCoverImage(string? coverImagePath, DateTimeOffset utcNow)
    {
        CoverImagePath = NormalizeOptionalText(coverImagePath, "Cover image path", 500);
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
            throw new DomainException("A published project requires a title and slug.");
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
