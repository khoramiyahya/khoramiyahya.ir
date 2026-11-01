using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Awards;
using PersonalBrand.Application.Awards.Contracts;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Domain.Awards;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Awards;

internal sealed class AwardService : IAwardService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateAwardRequest> _createValidator;
    private readonly IValidator<UpdateAwardRequest> _updateValidator;

    public AwardService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateAwardRequest> createValidator,
        IValidator<UpdateAwardRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<AwardDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var awards = await _dbContext.Awards
            .AsNoTracking()
            .Where(award => award.IsPublished)
            .OrderByDescending(award => award.IsFeatured)
            .ThenByDescending(award => award.AwardedOn)
            .ThenBy(award => award.DisplayOrder)
            .ToListAsync(cancellationToken);

        return awards.Select(Map).ToArray();
    }

    public async Task<AwardDto> GetPublishedByIdAsync(
        Guid awardId,
        CancellationToken cancellationToken = default)
    {
        var award = await _dbContext.Awards
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == awardId && entry.IsPublished,
                cancellationToken);

        if (award is null)
        {
            throw new NotFoundException("Award", awardId);
        }

        return Map(award);
    }

    public async Task<IReadOnlyList<AwardDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var awards = await _dbContext.Awards
            .AsNoTracking()
            .Where(award => award.UserId == userId)
            .OrderByDescending(award => award.AwardedOn)
            .ThenBy(award => award.DisplayOrder)
            .ToListAsync(cancellationToken);

        return awards.Select(Map).ToArray();
    }

    public async Task<AwardDto> CreateMineAsync(
        CreateAwardRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Award award;
        try
        {
            award = Award.Create(userId, request.Title, request.Issuer, request.AwardedOn, utcNow);
            award.UpdateDetails(
                title: request.Title,
                issuer: request.Issuer,
                description: request.Description,
                url: request.Url,
                awardedOn: request.AwardedOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                award.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Awards.Add(award);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "award.create",
            entityName: nameof(Award),
            entityId: award.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(award);
    }

    public async Task<AwardDto> UpdateMineAsync(
        Guid awardId,
        UpdateAwardRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var award = await GetOwnedAsync(userId, awardId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            award.UpdateDetails(
                title: request.Title,
                issuer: request.Issuer,
                description: request.Description,
                url: request.Url,
                awardedOn: request.AwardedOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                award.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                award.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "award.update",
            entityName: nameof(Award),
            entityId: award.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(award);
    }

    public async Task DeleteMineAsync(Guid awardId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var award = await GetOwnedAsync(userId, awardId, cancellationToken);

        _dbContext.Awards.Remove(award);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "award.delete",
            entityName: nameof(Award),
            entityId: award.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<AwardDto> PublishMineAsync(
        Guid awardId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var award = await GetOwnedAsync(userId, awardId, cancellationToken);

        try
        {
            award.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "award.publish",
            entityName: nameof(Award),
            entityId: award.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(award);
    }

    public async Task<AwardDto> UnpublishMineAsync(
        Guid awardId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var award = await GetOwnedAsync(userId, awardId, cancellationToken);

        award.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "award.unpublish",
            entityName: nameof(Award),
            entityId: award.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(award);
    }

    private async Task<Award> GetOwnedAsync(
        string userId,
        Guid awardId,
        CancellationToken cancellationToken)
    {
        var award = await _dbContext.Awards
            .SingleOrDefaultAsync(
                entry => entry.Id == awardId && entry.UserId == userId,
                cancellationToken);

        if (award is null)
        {
            throw new NotFoundException("Award", awardId);
        }

        return award;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static AwardDto Map(Award award) =>
        new()
        {
            Id = award.Id,
            Title = award.Title,
            Issuer = award.Issuer,
            Description = award.Description,
            Url = award.Url,
            AwardedOn = award.AwardedOn,
            DisplayOrder = award.DisplayOrder,
            IsFeatured = award.IsFeatured,
            IsPublished = award.IsPublished,
            UpdatedAtUtc = award.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Award"] = [exception.Message]
        });
}
