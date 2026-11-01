using FluentValidation;

using Microsoft.AspNetCore.Identity;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Auth;
using PersonalBrand.Application.Auth.Contracts;
using PersonalBrand.Application.Authorization;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Infrastructure.Identity;

namespace PersonalBrand.Infrastructure.Auth;

internal sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await _registerValidator.EnsureValidAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim(),
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw MapIdentityErrors(createResult.Errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.Viewer);
        if (!roleResult.Succeeded)
        {
            throw MapIdentityErrors(roleResult.Errors);
        }

        await _auditLogger.LogAsync(
            action: "auth.register",
            entityName: nameof(ApplicationUser),
            entityId: user.Id,
            cancellationToken: cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await _loginValidator.EnsureValidAsync(request, cancellationToken);

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            throw new UnauthorizedAppException("This account is temporarily locked. Try again later.");
        }

        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        user.LastLoginAtUtc = _dateTimeProvider.UtcNow;
        await _userManager.UpdateAsync(user);

        await _auditLogger.LogAsync(
            action: "auth.login",
            entityName: nameof(ApplicationUser),
            entityId: user.Id,
            cancellationToken: cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAppException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapUser(user, roles);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc, expiresInSeconds) = _jwtTokenService.CreateToken(user, roles);

        return new AuthResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresInSeconds = expiresInSeconds,
            ExpiresAtUtc = expiresAtUtc,
            User = MapUser(user, roles)
        };
    }

    private static UserDto MapUser(ApplicationUser user, IList<string> roles) =>
        new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Roles = roles.ToArray()
        };

    private static AppException MapIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var identityErrors = errors.ToArray();
        if (identityErrors.Length == 0)
        {
            return new AppException("Identity operation failed.");
        }

        var lookup = identityErrors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.Code) ? "Identity" : error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

        if (identityErrors.Any(error =>
                error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
                || error.Description.Contains("already taken", StringComparison.OrdinalIgnoreCase)))
        {
            return new ConflictException(identityErrors[0].Description);
        }

        return new AppValidationException(lookup);
    }
}
