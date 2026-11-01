using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Media;

/// <summary>
/// Media library asset owned by a user account.
/// </summary>
public sealed class MediaAsset : AggregateRoot<Guid>
{
    private MediaAsset()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string StoredPath { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public string? Title { get; private set; }

    public string? AltText { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static MediaAsset Create(
        string userId,
        string fileName,
        string storedPath,
        string contentType,
        long sizeBytes,
        DateTimeOffset utcNow,
        string? title = null,
        string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Media owner is required.");
        }

        if (sizeBytes <= 0)
        {
            throw new DomainException("Media file size must be greater than zero.");
        }

        var asset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            FileName = NormalizeRequiredText(fileName, "File name", 255),
            StoredPath = NormalizeRequiredText(storedPath, "Stored path", 500),
            ContentType = NormalizeRequiredText(contentType, "Content type", 100),
            SizeBytes = sizeBytes,
            Title = NormalizeOptionalText(title, "Title", 200),
            AltText = NormalizeOptionalText(altText, "Alt text", 300),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        return asset;
    }

    public void UpdateDetails(string? title, string? altText, DateTimeOffset utcNow)
    {
        Title = NormalizeOptionalText(title, "Title", 200);
        AltText = NormalizeOptionalText(altText, "Alt text", 300);
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }
}
