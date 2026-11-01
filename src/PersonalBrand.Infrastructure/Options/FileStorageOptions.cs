namespace PersonalBrand.Infrastructure.Options;

/// <summary>
/// Local file storage and image upload settings.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Absolute or content-root-relative path for uploaded files.
    /// </summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    /// <summary>
    /// Public URL path prefix used to serve uploaded files.
    /// </summary>
    public string RequestPath { get; set; } = "/uploads";

    /// <summary>
    /// Maximum avatar size in bytes.
    /// </summary>
    public long AvatarMaxBytes { get; set; } = 2 * 1024 * 1024;

    public string[] AllowedAvatarContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    public string[] AllowedAvatarExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    ];

    /// <summary>
    /// Maximum project cover image size in bytes.
    /// </summary>
    public long CoverMaxBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedCoverContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    public string[] AllowedCoverExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    ];
}
