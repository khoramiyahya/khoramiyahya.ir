using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Navigation;
using PersonalBrand.Application.Navigation.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Navigation;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Navigation;

internal sealed class NavigationService : INavigationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateNavigationItemRequest> _createValidator;
    private readonly IValidator<UpdateNavigationItemRequest> _updateValidator;

    public NavigationService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateNavigationItemRequest> createValidator,
        IValidator<UpdateNavigationItemRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<NavigationItemDto>> GetPublishedAsync(
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.NavigationItems
            .AsNoTracking()
            .Where(item => item.IsPublished);

        if (!string.IsNullOrWhiteSpace(location))
        {
            var normalizedLocation = location.Trim();
            query = query.Where(item => item.Location == normalizedLocation);
        }

        var items = await query
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Label)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<NavigationItemDto> GetPublishedByIdAsync(
        Guid navigationItemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.NavigationItems
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == navigationItemId && entry.IsPublished,
                cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("NavigationItem", navigationItemId);
        }

        return Map(item);
    }

    public async Task<IReadOnlyList<NavigationItemDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var items = await _dbContext.NavigationItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Label)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<NavigationItemDto> CreateMineAsync(
        CreateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        NavigationItem item;
        try
        {
            item = NavigationItem.Create(userId, request.Label, request.Url, utcNow);
            item.UpdateDetails(
                label: request.Label,
                url: request.Url,
                location: request.Location,
                opensInNewTab: request.OpensInNewTab,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                item.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.NavigationItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "navigation.create",
            entityName: nameof(NavigationItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task<NavigationItemDto> UpdateMineAsync(
        Guid navigationItemId,
        UpdateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, navigationItemId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            item.UpdateDetails(
                label: request.Label,
                url: request.Url,
                location: request.Location,
                opensInNewTab: request.OpensInNewTab,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                item.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                item.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "navigation.update",
            entityName: nameof(NavigationItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task DeleteMineAsync(Guid navigationItemId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, navigationItemId, cancellationToken);

        _dbContext.NavigationItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "navigation.delete",
            entityName: nameof(NavigationItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<NavigationItemDto> PublishMineAsync(
        Guid navigationItemId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, navigationItemId, cancellationToken);

        try
        {
            item.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "navigation.publish",
            entityName: nameof(NavigationItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task<NavigationItemDto> UnpublishMineAsync(
        Guid navigationItemId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, navigationItemId, cancellationToken);

        item.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "navigation.unpublish",
            entityName: nameof(NavigationItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    private async Task<NavigationItem> GetOwnedAsync(
        string userId,
        Guid navigationItemId,
        CancellationToken cancellationToken)
    {
        var item = await _dbContext.NavigationItems
            .SingleOrDefaultAsync(
                entry => entry.Id == navigationItemId && entry.UserId == userId,
                cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("NavigationItem", navigationItemId);
        }

        return item;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static NavigationItemDto Map(NavigationItem item) =>
        new()
        {
            Id = item.Id,
            Label = item.Label,
            Url = item.Url,
            Location = item.Location,
            OpensInNewTab = item.OpensInNewTab,
            DisplayOrder = item.DisplayOrder,
            IsPublished = item.IsPublished,
            UpdatedAtUtc = item.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Navigation"] = [exception.Message]
        });
}
