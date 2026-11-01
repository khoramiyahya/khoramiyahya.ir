namespace PersonalBrand.Application.Authorization;

/// <summary>
/// Built-in application role names.
/// </summary>
public static class AppRoles
{
    public const string Administrator = "Administrator";

    public const string Editor = "Editor";

    public const string Viewer = "Viewer";

    public static IReadOnlyList<string> All { get; } =
    [
        Administrator,
        Editor,
        Viewer
    ];
}
