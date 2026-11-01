namespace PersonalBrand.Application.Services.Contracts;

/// <summary>
/// Public representation of a service offering.
/// </summary>
public sealed class ServiceOfferDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Description { get; init; }

    public string? PriceLabel { get; init; }

    public string? IconName { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
