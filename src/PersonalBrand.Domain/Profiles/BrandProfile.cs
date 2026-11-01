using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Profiles;

/// <summary>
/// Personal brand profile aggregate owned by a single user account.
/// </summary>
public sealed class BrandProfile : AggregateRoot<Guid>
{
    private BrandProfile()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Headline { get; private set; }

    public string? Bio { get; private set; }

    public string? AvatarPath { get; private set; }

    public string? Location { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public string? LinkedInUrl { get; private set; }

    public string? GitHubUrl { get; private set; }

    public string? TwitterUrl { get; private set; }

    public string? InstagramUrl { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static BrandProfile Create(string userId, string displayName, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Profile owner is required.");
        }

        var normalizedDisplayName = NormalizeRequiredText(displayName, "Display name", 200);
        var profile = new BrandProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            DisplayName = normalizedDisplayName,
            Slug = ProfileSlug.FromDisplayName(normalizedDisplayName),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            IsPublished = false
        };

        return profile;
    }

    public void UpdateDetails(
        string displayName,
        string? headline,
        string? bio,
        string? location,
        string? websiteUrl,
        string? linkedInUrl,
        string? gitHubUrl,
        string? twitterUrl,
        string? instagramUrl,
        string? slug,
        DateTimeOffset utcNow)
    {
        DisplayName = NormalizeRequiredText(displayName, "Display name", 200);
        Headline = NormalizeOptionalText(headline, "Headline", 300);
        Bio = NormalizeOptionalText(bio, "Bio", 4000);
        Location = NormalizeOptionalText(location, "Location", 200);
        WebsiteUrl = NormalizeOptionalUrl(websiteUrl, "Website URL");
        LinkedInUrl = NormalizeOptionalUrl(linkedInUrl, "LinkedIn URL");
        GitHubUrl = NormalizeOptionalUrl(gitHubUrl, "GitHub URL");
        TwitterUrl = NormalizeOptionalUrl(twitterUrl, "Twitter URL");
        InstagramUrl = NormalizeOptionalUrl(instagramUrl, "Instagram URL");

        if (!string.IsNullOrWhiteSpace(slug))
        {
            Slug = ProfileSlug.Normalize(slug);
        }

        Touch(utcNow);
    }

    public void SetAvatar(string? avatarPath, DateTimeOffset utcNow)
    {
        AvatarPath = NormalizeOptionalText(avatarPath, "Avatar path", 500);
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Slug))
        {
            throw new DomainException("A published profile requires a display name and slug.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    public void EnsureSlug(string slug, DateTimeOffset utcNow)
    {
        Slug = ProfileSlug.Normalize(slug);
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
