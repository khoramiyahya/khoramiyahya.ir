namespace PersonalBrand.Application.Education.Contracts;

/// <summary>
/// Public representation of an education entry.
/// </summary>
public sealed class EducationEntryDto
{
    public required Guid Id { get; init; }

    public required string Institution { get; init; }

    public required string Degree { get; init; }

    public string? FieldOfStudy { get; init; }

    public string? Location { get; init; }

    public string? Description { get; init; }

    public string? InstitutionUrl { get; init; }

    public required DateOnly StartedOn { get; init; }

    public DateOnly? EndedOn { get; init; }

    public required bool IsCurrent { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
