namespace PersonalBrand.Application.Skills.Contracts;

/// <summary>
/// Public representation of a skill.
/// </summary>
public sealed class SkillDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Category { get; init; }

    public string? Description { get; init; }

    public required int ProficiencyPercent { get; init; }

    public int? YearsExperience { get; init; }

    public string? IconName { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
