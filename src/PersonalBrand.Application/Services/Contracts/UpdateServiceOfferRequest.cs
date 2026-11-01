namespace PersonalBrand.Application.Services.Contracts;

/// <summary>
/// Request payload for updating a service offering.
/// </summary>
public sealed class UpdateServiceOfferRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public string? PriceLabel { get; set; }

    public string? IconName { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
