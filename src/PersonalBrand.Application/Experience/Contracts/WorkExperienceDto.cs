namespace PersonalBrand.Application.Experience.Contracts;

/// <summary>
/// Public representation of a work experience entry.
/// </summary>
public sealed class WorkExperienceDto
{
    public required Guid Id { get; init; }

    public required string Company { get; init; }

    public required string Title { get; init; }

    public string? Location { get; init; }

    public string? Description { get; init; }

    public string? CompanyUrl { get; init; }

    public required DateOnly StartedOn { get; init; }

    public DateOnly? EndedOn { get; init; }

    public required bool IsCurrent { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
