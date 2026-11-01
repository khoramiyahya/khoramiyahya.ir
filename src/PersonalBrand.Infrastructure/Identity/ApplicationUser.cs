using Microsoft.AspNetCore.Identity;

namespace PersonalBrand.Infrastructure.Identity;

/// <summary>
/// Application-specific Identity user.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}
