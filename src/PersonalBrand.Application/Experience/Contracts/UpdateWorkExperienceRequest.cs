namespace PersonalBrand.Application.Experience.Contracts;

/// <summary>
/// Request payload for updating a work experience entry.
/// </summary>
public sealed class UpdateWorkExperienceRequest
{
    public string Company { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? CompanyUrl { get; set; }

    public DateOnly StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    public bool IsCurrent { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
