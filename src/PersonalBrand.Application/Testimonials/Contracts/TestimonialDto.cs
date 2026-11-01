namespace PersonalBrand.Application.Testimonials.Contracts;

/// <summary>
/// Public representation of a testimonial.
/// </summary>
public sealed class TestimonialDto
{
    public required Guid Id { get; init; }

    public required string AuthorName { get; init; }

    public string? AuthorRole { get; init; }

    public string? Company { get; init; }

    public required string Quote { get; init; }

    public int? Rating { get; init; }

    public string? AuthorImageUrl { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
