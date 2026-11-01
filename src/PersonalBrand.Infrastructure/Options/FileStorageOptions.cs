namespace PersonalBrand.Infrastructure.Options;

/// <summary>
/// Local file storage settings.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Absolute or content-root-relative path for uploaded files.
    /// </summary>
    public string RootPath { get; set; } = "App_Data/uploads";
}
