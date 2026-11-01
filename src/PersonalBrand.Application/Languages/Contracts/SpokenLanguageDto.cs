namespace PersonalBrand.Application.Languages.Contracts;

/// <summary>
/// Public representation of a spoken language.
/// </summary>
public sealed class SpokenLanguageDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Proficiency { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
