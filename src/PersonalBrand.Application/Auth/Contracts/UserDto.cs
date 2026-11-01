namespace PersonalBrand.Application.Auth.Contracts;

/// <summary>
/// Public user profile returned by authentication endpoints.
/// </summary>
public sealed class UserDto
{
    public required string Id { get; init; }

    public required string Email { get; init; }

    public string? DisplayName { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
