using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Faq;
using PersonalBrand.Application.Faq.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Faq;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Faq;

internal sealed class FaqService : IFaqService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateFaqItemRequest> _createValidator;
    private readonly IValidator<UpdateFaqItemRequest> _updateValidator;

    public FaqService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateFaqItemRequest> createValidator,
        IValidator<UpdateFaqItemRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<FaqItemDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.FaqItems
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .OrderByDescending(item => item.IsFeatured)
            .ThenBy(item => item.DisplayOrder)
            .ThenBy(item => item.Question)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<FaqItemDto> GetPublishedByIdAsync(
        Guid faqId,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.FaqItems
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == faqId && entry.IsPublished,
                cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("FaqItem", faqId);
        }

        return Map(item);
    }

    public async Task<IReadOnlyList<FaqItemDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var items = await _dbContext.FaqItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Question)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<FaqItemDto> CreateMineAsync(
        CreateFaqItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        FaqItem item;
        try
        {
            item = FaqItem.Create(userId, request.Question, request.Answer, utcNow);
            item.UpdateDetails(
                question: request.Question,
                answer: request.Answer,
                category: request.Category,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
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

        _dbContext.FaqItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "faq.create",
            entityName: nameof(FaqItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task<FaqItemDto> UpdateMineAsync(
        Guid faqId,
        UpdateFaqItemRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, faqId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            item.UpdateDetails(
                question: request.Question,
                answer: request.Answer,
                category: request.Category,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
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
            action: "faq.update",
            entityName: nameof(FaqItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task DeleteMineAsync(Guid faqId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, faqId, cancellationToken);

        _dbContext.FaqItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "faq.delete",
            entityName: nameof(FaqItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<FaqItemDto> PublishMineAsync(
        Guid faqId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, faqId, cancellationToken);

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
            action: "faq.publish",
            entityName: nameof(FaqItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    public async Task<FaqItemDto> UnpublishMineAsync(
        Guid faqId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var item = await GetOwnedAsync(userId, faqId, cancellationToken);

        item.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "faq.unpublish",
            entityName: nameof(FaqItem),
            entityId: item.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(item);
    }

    private async Task<FaqItem> GetOwnedAsync(
        string userId,
        Guid faqId,
        CancellationToken cancellationToken)
    {
        var item = await _dbContext.FaqItems
            .SingleOrDefaultAsync(
                entry => entry.Id == faqId && entry.UserId == userId,
                cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("FaqItem", faqId);
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

    private static FaqItemDto Map(FaqItem item) =>
        new()
        {
            Id = item.Id,
            Question = item.Question,
            Answer = item.Answer,
            Category = item.Category,
            DisplayOrder = item.DisplayOrder,
            IsFeatured = item.IsFeatured,
            IsPublished = item.IsPublished,
            UpdatedAtUtc = item.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Faq"] = [exception.Message]
        });
}
