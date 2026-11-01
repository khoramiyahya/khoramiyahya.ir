namespace PersonalBrand.Application.Skills.Contracts;

/// <summary>
/// Request payload for creating a skill.
/// </summary>
public sealed class CreateSkillRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public int ProficiencyPercent { get; set; }

    public int? YearsExperience { get; set; }

    public string? IconName { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
