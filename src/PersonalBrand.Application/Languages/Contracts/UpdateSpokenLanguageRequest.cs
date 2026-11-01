namespace PersonalBrand.Application.Languages.Contracts;

/// <summary>
/// Request payload for updating a spoken language.
/// </summary>
public sealed class UpdateSpokenLanguageRequest
{
    public string Name { get; set; } = string.Empty;

    public string Proficiency { get; set; } = "professional";

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
