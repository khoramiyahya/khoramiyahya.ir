using Microsoft.AspNetCore.Identity;

namespace PersonalBrand.Infrastructure.Identity;

/// <summary>
/// Application-specific Identity role.
/// </summary>
public sealed class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
