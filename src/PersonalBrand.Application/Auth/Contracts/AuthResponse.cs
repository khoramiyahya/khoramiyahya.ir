namespace PersonalBrand.Application.Auth.Contracts;

/// <summary>
/// Authentication result containing the issued tokens and user profile.
/// </summary>
public sealed class AuthResponse
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required string TokenType { get; init; }

    public required int ExpiresInSeconds { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public required DateTimeOffset RefreshTokenExpiresAtUtc { get; init; }

    public required UserDto User { get; init; }
}
