namespace PersonalBrand.Application.Publications.Contracts;

/// <summary>
/// Request payload for creating a publication.
/// </summary>
public sealed class CreatePublicationRequest
{
    public string Title { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string PublicationType { get; set; } = "article";

    public string? Description { get; set; }

    public string? Url { get; set; }

    public DateOnly PublishedOn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
