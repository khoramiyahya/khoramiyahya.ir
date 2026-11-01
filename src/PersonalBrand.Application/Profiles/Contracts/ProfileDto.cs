namespace PersonalBrand.Application.Profiles.Contracts;

/// <summary>
/// Public representation of a brand profile.
/// </summary>
public sealed class ProfileDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string DisplayName { get; init; }

    public string? Headline { get; init; }

    public string? Bio { get; init; }

    /// <summary>
    /// Storage-relative avatar path.
    /// </summary>
    public string? AvatarPath { get; init; }

    /// <summary>
    /// Public URL for the avatar, when available.
    /// </summary>
    public string? AvatarUrl { get; init; }

    public string? Location { get; init; }

    public string? WebsiteUrl { get; init; }

    public string? LinkedInUrl { get; init; }

    public string? GitHubUrl { get; init; }

    public string? TwitterUrl { get; init; }

    public string? InstagramUrl { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
