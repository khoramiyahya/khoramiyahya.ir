namespace PersonalBrand.Application.Partners.Contracts;

/// <summary>
/// Public representation of a partner or client logo.
/// </summary>
public sealed class PartnerDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? WebsiteUrl { get; init; }

    public string? LogoUrl { get; init; }

    public string? Summary { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
