namespace PersonalBrand.Application.Navigation.Contracts;

/// <summary>
/// Request payload for updating a navigation menu item.
/// </summary>
public sealed class UpdateNavigationItemRequest
{
    public string Label { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Location { get; set; }

    public bool OpensInNewTab { get; set; }

    public int DisplayOrder { get; set; }

    public bool? IsPublished { get; set; }
}
