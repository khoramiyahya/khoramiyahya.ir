namespace PersonalBrand.Application.Talks.Contracts;

/// <summary>
/// Request payload for creating a talk.
/// </summary>
public sealed class CreateTalkRequest
{
    public string Title { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public DateOnly SpokenOn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
