namespace PersonalBrand.Application.Testimonials.Contracts;

/// <summary>
/// Request payload for creating a testimonial.
/// </summary>
public sealed class CreateTestimonialRequest
{
    public string AuthorName { get; set; } = string.Empty;

    public string? AuthorRole { get; set; }

    public string? Company { get; set; }

    public string Quote { get; set; } = string.Empty;

    public int? Rating { get; set; }

    public string? AuthorImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
