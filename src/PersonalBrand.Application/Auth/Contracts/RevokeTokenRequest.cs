namespace PersonalBrand.Application.Auth.Contracts;

/// <summary>
/// Request payload for revoking a refresh token.
/// </summary>
public sealed class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
