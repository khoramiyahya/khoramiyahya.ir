using PersonalBrand.Application.Auth.Contracts;

namespace PersonalBrand.Application.Auth;

/// <summary>
/// Application authentication use cases.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
