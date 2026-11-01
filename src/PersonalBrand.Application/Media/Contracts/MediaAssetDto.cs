namespace PersonalBrand.Application.Media.Contracts;

/// <summary>
/// Representation of a media library asset.
/// </summary>
public sealed class MediaAssetDto
{
    public required Guid Id { get; init; }

    public required string FileName { get; init; }

    public required string StoredPath { get; init; }

    public required string Url { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }

    public string? Title { get; init; }

    public string? AltText { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
