namespace PersonalBrand.Application.Redirects.Contracts;

/// <summary>
/// Public representation of an SEO redirect rule.
/// </summary>
public sealed class SeoRedirectDto
{
    public required Guid Id { get; init; }

    public required string FromPath { get; init; }

    public required string ToUrl { get; init; }

    public required int StatusCode { get; init; }

    public string? Notes { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
