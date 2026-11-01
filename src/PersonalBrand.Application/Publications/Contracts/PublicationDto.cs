namespace PersonalBrand.Application.Publications.Contracts;

/// <summary>
/// Public representation of an external publication.
/// </summary>
public sealed class PublicationDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Publisher { get; init; }

    public required string PublicationType { get; init; }

    public string? Description { get; init; }

    public string? Url { get; init; }

    public required DateOnly PublishedOn { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
