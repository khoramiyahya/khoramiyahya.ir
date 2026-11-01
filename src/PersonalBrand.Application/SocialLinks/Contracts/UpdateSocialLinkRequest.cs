namespace PersonalBrand.Application.SocialLinks.Contracts;

/// <summary>
/// Request payload for updating a social link.
/// </summary>
public sealed class UpdateSocialLinkRequest
{
    public string Platform { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool? IsPublished { get; set; }
}
