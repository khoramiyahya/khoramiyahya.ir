using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Pages;
using PersonalBrand.Application.Pages.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Pages;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Pages;

internal sealed class PageService : IPageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateContentPageRequest> _createValidator;
    private readonly IValidator<UpdateContentPageRequest> _updateValidator;

    public PageService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateContentPageRequest> createValidator,
        IValidator<UpdateContentPageRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ContentPageDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var pages = await _dbContext.ContentPages
            .AsNoTracking()
            .Where(page => page.IsPublished)
            .OrderBy(page => page.DisplayOrder)
            .ThenBy(page => page.Title)
            .ToListAsync(cancellationToken);

        return pages.Select(Map).ToArray();
    }

    public async Task<ContentPageDto> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException("Published page was not found.");
        }

        string normalizedSlug;
        try
        {
            normalizedSlug = ProfileSlug.Normalize(slug);
        }
        catch (DomainException)
        {
            throw new NotFoundException("Published page was not found.");
        }

        var page = await _dbContext.ContentPages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug && item.IsPublished,
                cancellationToken);

        if (page is null)
        {
            throw new NotFoundException("ContentPage", normalizedSlug);
        }

        return Map(page);
    }

    public async Task<IReadOnlyList<ContentPageDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var pages = await _dbContext.ContentPages
            .AsNoTracking()
            .Where(page => page.UserId == userId)
            .OrderBy(page => page.DisplayOrder)
            .ThenByDescending(page => page.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return pages.Select(Map).ToArray();
    }

    public async Task<ContentPageDto> CreateMineAsync(
        CreateContentPageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        ContentPage page;
        try
        {
            page = ContentPage.Create(userId, request.Title, utcNow, request.Slug);
            await EnsureUniqueSlugAsync(page, cancellationToken);

            page.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                body: request.Body,
                metaTitle: request.MetaTitle,
                metaDescription: request.MetaDescription,
                displayOrder: request.DisplayOrder,
                slug: null,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                page.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.ContentPages.Add(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "page.create",
            entityName: nameof(ContentPage),
            entityId: page.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(page);
    }

    public async Task<ContentPageDto> UpdateMineAsync(
        Guid pageId,
        UpdateContentPageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var page = await GetOwnedAsync(userId, pageId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? page.Slug
                : ProfileSlug.Normalize(request.Slug);

            await EnsureSlugIsAvailableAsync(desiredSlug, page.Id, cancellationToken);

            page.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                body: request.Body,
                metaTitle: request.MetaTitle,
                metaDescription: request.MetaDescription,
                displayOrder: request.DisplayOrder,
                slug: desiredSlug,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                page.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                page.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "page.update",
            entityName: nameof(ContentPage),
            entityId: page.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(page);
    }

    public async Task DeleteMineAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var page = await GetOwnedAsync(userId, pageId, cancellationToken);

        _dbContext.ContentPages.Remove(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "page.delete",
            entityName: nameof(ContentPage),
            entityId: page.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<ContentPageDto> PublishMineAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var page = await GetOwnedAsync(userId, pageId, cancellationToken);

        try
        {
            page.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "page.publish",
            entityName: nameof(ContentPage),
            entityId: page.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(page);
    }

    public async Task<ContentPageDto> UnpublishMineAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var page = await GetOwnedAsync(userId, pageId, cancellationToken);

        page.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "page.unpublish",
            entityName: nameof(ContentPage),
            entityId: page.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(page);
    }

    private async Task EnsureUniqueSlugAsync(ContentPage page, CancellationToken cancellationToken)
    {
        var baseSlug = page.Slug;
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.ContentPages.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        if (!string.Equals(candidate, page.Slug, StringComparison.Ordinal))
        {
            page.EnsureSlug(candidate, _dateTimeProvider.UtcNow);
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid currentPageId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ContentPages
            .AnyAsync(
                page => page.Slug == slug && page.Id != currentPageId,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Slug '{slug}' is already in use.");
        }
    }

    private async Task<ContentPage> GetOwnedAsync(
        string userId,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var page = await _dbContext.ContentPages
            .SingleOrDefaultAsync(
                item => item.Id == pageId && item.UserId == userId,
                cancellationToken);

        if (page is null)
        {
            throw new NotFoundException("ContentPage", pageId);
        }

        return page;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static ContentPageDto Map(ContentPage page) =>
        new()
        {
            Id = page.Id,
            Slug = page.Slug,
            Title = page.Title,
            Summary = page.Summary,
            Body = page.Body,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            DisplayOrder = page.DisplayOrder,
            IsPublished = page.IsPublished,
            PublishedAtUtc = page.PublishedAtUtc,
            UpdatedAtUtc = page.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Page"] = [exception.Message]
        });
}
