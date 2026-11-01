namespace PersonalBrand.Application.Navigation.Contracts;

/// <summary>
/// Public representation of a navigation menu item.
/// </summary>
public sealed class NavigationItemDto
{
    public required Guid Id { get; init; }

    public required string Label { get; init; }

    public required string Url { get; init; }

    public string? Location { get; init; }

    public required bool OpensInNewTab { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
