namespace PersonalBrand.Application.Awards.Contracts;

/// <summary>
/// Public representation of an award.
/// </summary>
public sealed class AwardDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Issuer { get; init; }

    public string? Description { get; init; }

    public string? Url { get; init; }

    public required DateOnly AwardedOn { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
