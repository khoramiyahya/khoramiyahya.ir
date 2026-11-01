namespace PersonalBrand.Application.Languages.Contracts;

/// <summary>
/// Request payload for creating a spoken language.
/// </summary>
public sealed class CreateSpokenLanguageRequest
{
    public string Name { get; set; } = string.Empty;

    public string Proficiency { get; set; } = "professional";

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
