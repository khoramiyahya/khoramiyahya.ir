namespace PersonalBrand.Application.Auth.Contracts;

/// <summary>
/// Request payload for registering a new user account.
/// </summary>
public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}
