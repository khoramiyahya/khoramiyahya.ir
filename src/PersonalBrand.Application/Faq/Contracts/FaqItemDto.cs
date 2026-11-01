namespace PersonalBrand.Application.Faq.Contracts;

/// <summary>
/// Public representation of an FAQ item.
/// </summary>
public sealed class FaqItemDto
{
    public required Guid Id { get; init; }

    public required string Question { get; init; }

    public required string Answer { get; init; }

    public string? Category { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
