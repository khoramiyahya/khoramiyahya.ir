namespace PersonalBrand.Application.Abstractions;

/// <summary>
/// Provides identity information for the current request principal.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }
}
