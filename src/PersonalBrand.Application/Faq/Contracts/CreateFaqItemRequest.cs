namespace PersonalBrand.Application.Faq.Contracts;

/// <summary>
/// Request payload for creating an FAQ item.
/// </summary>
public sealed class CreateFaqItemRequest
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string? Category { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
