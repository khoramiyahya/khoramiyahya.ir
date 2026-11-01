namespace PersonalBrand.Application.Education.Contracts;

/// <summary>
/// Request payload for creating an education entry.
/// </summary>
public sealed class CreateEducationEntryRequest
{
    public string Institution { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public string? FieldOfStudy { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? InstitutionUrl { get; set; }

    public DateOnly StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    public bool IsCurrent { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
