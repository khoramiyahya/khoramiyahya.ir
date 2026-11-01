namespace PersonalBrand.Application.Partners.Contracts;

/// <summary>
/// Request payload for updating a partner.
/// </summary>
public sealed class UpdatePartnerRequest
{
    public string Name { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? Summary { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
