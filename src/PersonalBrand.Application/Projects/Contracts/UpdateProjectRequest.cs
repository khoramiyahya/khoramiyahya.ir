namespace PersonalBrand.Application.Projects.Contracts;

/// <summary>
/// Request payload for updating a portfolio project.
/// </summary>
public sealed class UpdateProjectRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public string? ProjectUrl { get; set; }

    public string? RepoUrl { get; set; }

    public string? Technologies { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }

    public DateOnly? StartedOn { get; set; }

    public DateOnly? CompletedOn { get; set; }
}
