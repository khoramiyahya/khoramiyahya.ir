namespace PersonalBrand.Application.Pages.Contracts;

/// <summary>
/// Request payload for creating a content page.
/// </summary>
public sealed class CreateContentPageRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Body { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
