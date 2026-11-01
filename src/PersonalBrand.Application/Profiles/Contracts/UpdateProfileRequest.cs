namespace PersonalBrand.Application.Profiles.Contracts;

/// <summary>
/// Request payload for creating or updating the current user's brand profile.
/// </summary>
public sealed class UpdateProfileRequest
{
    public string DisplayName { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Headline { get; set; }

    public string? Bio { get; set; }

    public string? Location { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public string? TwitterUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public bool? IsPublished { get; set; }
}
