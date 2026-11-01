namespace PersonalBrand.Application.Awards.Contracts;

/// <summary>
/// Request payload for creating an award.
/// </summary>
public sealed class CreateAwardRequest
{
    public string Title { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public DateOnly AwardedOn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
