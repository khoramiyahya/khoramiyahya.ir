namespace PersonalBrand.Application.SocialLinks.Contracts;

/// <summary>
/// Public representation of a social link.
/// </summary>
public sealed class SocialLinkDto
{
    public required Guid Id { get; init; }

    public required string Platform { get; init; }

    public required string Label { get; init; }

    public required string Url { get; init; }

    public string? Icon { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
