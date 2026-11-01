namespace PersonalBrand.Application.Pages.Contracts;

/// <summary>
/// Request payload for updating a content page.
/// </summary>
public sealed class UpdateContentPageRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Body { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public int DisplayOrder { get; set; }

    public bool? IsPublished { get; set; }
}
