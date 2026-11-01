using System.Security.Cryptography;
using System.Text;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Auth;
using PersonalBrand.Application.Auth.Contracts;
using PersonalBrand.Application.Authorization;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Options;
using PersonalBrand.Infrastructure.Identity;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Auth;

internal sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<RevokeTokenRequest> _revokeValidator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        JwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        IValidator<RevokeTokenRequest> revokeValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _revokeValidator = revokeValidator;
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

        return await CreateAuthResponseAsync(user, familyId: Guid.NewGuid(), cancellationToken);
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

        return await CreateAuthResponseAsync(user, familyId: Guid.NewGuid(), cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await _refreshValidator.EnsureValidAsync(request, cancellationToken);

        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        if (existingToken.IsRevoked)
        {
            // A rotated token being presented again indicates theft/reuse.
            if (!string.IsNullOrWhiteSpace(existingToken.ReplacedByTokenHash))
            {
                await RevokeFamilyAsync(existingToken.FamilyId, cancellationToken);
                throw new UnauthorizedAppException("Refresh token reuse detected.");
            }

            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        if (existingToken.IsExpired(_dateTimeProvider.UtcNow))
        {
            throw new UnauthorizedAppException("Refresh token has expired.");
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId);
        if (user is null || !user.IsActive)
        {
            await RevokeFamilyAsync(existingToken.FamilyId, cancellationToken);
            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        var rawRefreshToken = CreateRawRefreshToken();
        var newTokenHash = HashToken(rawRefreshToken);
        var refreshExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        existingToken.RevokedAtUtc = _dateTimeProvider.UtcNow;
        existingToken.ReplacedByTokenHash = newTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            FamilyId = existingToken.FamilyId,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            ExpiresAtUtc = refreshExpiresAtUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "auth.refresh",
            entityName: nameof(ApplicationUser),
            entityId: user.Id,
            cancellationToken: cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAtUtc, expiresInSeconds) = _jwtTokenService.CreateToken(user, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            TokenType = "Bearer",
            ExpiresInSeconds = expiresInSeconds,
            ExpiresAtUtc = expiresAtUtc,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc,
            User = MapUser(user, roles)
        };
    }

    public async Task RevokeAsync(
        RevokeTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await _revokeValidator.EnsureValidAsync(request, cancellationToken);

        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || existingToken.IsRevoked)
        {
            return;
        }

        existingToken.RevokedAtUtc = _dateTimeProvider.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "auth.revoke",
            entityName: nameof(RefreshToken),
            entityId: existingToken.Id.ToString(),
            cancellationToken: cancellationToken);
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
        Guid familyId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAtUtc, expiresInSeconds) = _jwtTokenService.CreateToken(user, roles);

        var rawRefreshToken = CreateRawRefreshToken();
        var refreshExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            FamilyId = familyId,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            ExpiresAtUtc = refreshExpiresAtUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            TokenType = "Bearer",
            ExpiresInSeconds = expiresInSeconds,
            ExpiresAtUtc = expiresAtUtc,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc,
            User = MapUser(user, roles)
        };
    }

    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var familyTokens = await _dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        foreach (var token in familyTokens)
        {
            token.RevokedAtUtc = now;
        }

        if (familyTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string CreateRawRefreshToken()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
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
