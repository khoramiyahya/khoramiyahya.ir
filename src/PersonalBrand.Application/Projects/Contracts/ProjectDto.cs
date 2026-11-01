namespace PersonalBrand.Application.Projects.Contracts;

/// <summary>
/// Public representation of a portfolio project.
/// </summary>
public sealed class ProjectDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Description { get; init; }

    public string? CoverImagePath { get; init; }

    public string? CoverImageUrl { get; init; }

    public string? ProjectUrl { get; init; }

    public string? RepoUrl { get; init; }

    public string? Technologies { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public DateOnly? StartedOn { get; init; }

    public DateOnly? CompletedOn { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
