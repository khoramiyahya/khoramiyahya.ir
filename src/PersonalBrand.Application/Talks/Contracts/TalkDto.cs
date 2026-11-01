namespace PersonalBrand.Application.Talks.Contracts;

/// <summary>
/// Public representation of a speaking engagement.
/// </summary>
public sealed class TalkDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string EventName { get; init; }

    public string? Location { get; init; }

    public string? Description { get; init; }

    public string? Url { get; init; }

    public required DateOnly SpokenOn { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
