namespace PersonalBrand.Application.Auth.Contracts;

/// <summary>
/// Request payload for exchanging a refresh token for a new token pair.
/// </summary>
public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
