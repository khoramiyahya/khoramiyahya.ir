using PersonalBrand.Application.Authorization;

namespace PersonalBrand.Infrastructure.Identity;

/// <summary>
/// Infrastructure alias for application role names.
/// </summary>
public static class IdentityRoles
{
    public const string Administrator = AppRoles.Administrator;

    public const string Editor = AppRoles.Editor;

    public const string Viewer = AppRoles.Viewer;

    public static IReadOnlyList<string> All => AppRoles.All;
}
