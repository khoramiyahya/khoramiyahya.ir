namespace PersonalBrand.Infrastructure.Options;

/// <summary>
/// Optional bootstrap administrator created once when roles are seeded.
/// Leave Email/Password empty to skip seeding an admin account.
/// </summary>
public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = "Administrator";
}
