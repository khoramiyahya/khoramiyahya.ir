using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Redirects;
using PersonalBrand.Application.Redirects.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Redirects;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Redirects;

internal sealed class RedirectService : IRedirectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateSeoRedirectRequest> _createValidator;
    private readonly IValidator<UpdateSeoRedirectRequest> _updateValidator;

    public RedirectService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateSeoRedirectRequest> createValidator,
        IValidator<UpdateSeoRedirectRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<SeoRedirectDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var redirects = await _dbContext.SeoRedirects
            .AsNoTracking()
            .Where(redirect => redirect.IsPublished)
            .OrderBy(redirect => redirect.FromPath)
            .ToListAsync(cancellationToken);

        return redirects.Select(Map).ToArray();
    }

    public async Task<SeoRedirectDto> ResolvePublishedByPathAsync(
        string fromPath,
        CancellationToken cancellationToken = default)
    {
        var normalizedPath = NormalizeLookupPath(fromPath);
        var redirect = await _dbContext.SeoRedirects
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.FromPath == normalizedPath && entry.IsPublished,
                cancellationToken);

        if (redirect is null)
        {
            throw new NotFoundException("SeoRedirect", normalizedPath);
        }

        return Map(redirect);
    }

    public async Task<IReadOnlyList<SeoRedirectDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var redirects = await _dbContext.SeoRedirects
            .AsNoTracking()
            .Where(redirect => redirect.UserId == userId)
            .OrderBy(redirect => redirect.FromPath)
            .ToListAsync(cancellationToken);

        return redirects.Select(Map).ToArray();
    }

    public async Task<SeoRedirectDto> CreateMineAsync(
        CreateSeoRedirectRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        SeoRedirect redirect;
        try
        {
            redirect = SeoRedirect.Create(userId, request.FromPath, request.ToUrl, utcNow);
            redirect.UpdateDetails(
                fromPath: request.FromPath,
                toUrl: request.ToUrl,
                statusCode: request.StatusCode,
                notes: request.Notes,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                redirect.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await EnsureUniqueFromPathAsync(redirect.FromPath, excludeId: null, cancellationToken);

        _dbContext.SeoRedirects.Add(redirect);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "redirect.create",
            entityName: nameof(SeoRedirect),
            entityId: redirect.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(redirect);
    }

    public async Task<SeoRedirectDto> UpdateMineAsync(
        Guid redirectId,
        UpdateSeoRedirectRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var redirect = await GetOwnedAsync(userId, redirectId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            redirect.UpdateDetails(
                fromPath: request.FromPath,
                toUrl: request.ToUrl,
                statusCode: request.StatusCode,
                notes: request.Notes,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                redirect.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                redirect.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await EnsureUniqueFromPathAsync(redirect.FromPath, excludeId: redirect.Id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "redirect.update",
            entityName: nameof(SeoRedirect),
            entityId: redirect.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(redirect);
    }

    public async Task DeleteMineAsync(Guid redirectId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var redirect = await GetOwnedAsync(userId, redirectId, cancellationToken);

        _dbContext.SeoRedirects.Remove(redirect);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "redirect.delete",
            entityName: nameof(SeoRedirect),
            entityId: redirect.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<SeoRedirectDto> PublishMineAsync(
        Guid redirectId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var redirect = await GetOwnedAsync(userId, redirectId, cancellationToken);

        try
        {
            redirect.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "redirect.publish",
            entityName: nameof(SeoRedirect),
            entityId: redirect.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(redirect);
    }

    public async Task<SeoRedirectDto> UnpublishMineAsync(
        Guid redirectId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var redirect = await GetOwnedAsync(userId, redirectId, cancellationToken);

        redirect.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "redirect.unpublish",
            entityName: nameof(SeoRedirect),
            entityId: redirect.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(redirect);
    }

    private async Task EnsureUniqueFromPathAsync(
        string fromPath,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.SeoRedirects.AnyAsync(
            entry => entry.FromPath == fromPath && (!excludeId.HasValue || entry.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["FromPath"] = ["A redirect with this from path already exists."]
            });
        }
    }

    private async Task<SeoRedirect> GetOwnedAsync(
        string userId,
        Guid redirectId,
        CancellationToken cancellationToken)
    {
        var redirect = await _dbContext.SeoRedirects
            .SingleOrDefaultAsync(
                entry => entry.Id == redirectId && entry.UserId == userId,
                cancellationToken);

        if (redirect is null)
        {
            throw new NotFoundException("SeoRedirect", redirectId);
        }

        return redirect;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static string NormalizeLookupPath(string fromPath)
    {
        if (string.IsNullOrWhiteSpace(fromPath))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["FromPath"] = ["From path is required."]
            });
        }

        var normalized = fromPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized;
    }

    private static SeoRedirectDto Map(SeoRedirect redirect) =>
        new()
        {
            Id = redirect.Id,
            FromPath = redirect.FromPath,
            ToUrl = redirect.ToUrl,
            StatusCode = redirect.StatusCode,
            Notes = redirect.Notes,
            IsPublished = redirect.IsPublished,
            UpdatedAtUtc = redirect.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Redirect"] = [exception.Message]
        });
}
