namespace PersonalBrand.Application.Robots.Contracts;

/// <summary>
/// Public representation of a robots.txt rule.
/// </summary>
public sealed class RobotsRuleDto
{
    public required Guid Id { get; init; }

    public required string UserAgent { get; init; }

    public required string Directive { get; init; }

    public required string Path { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
