using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.SocialLinks;
using PersonalBrand.Application.SocialLinks.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.SocialLinks;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.SocialLinks;

internal sealed class SocialLinkService : ISocialLinkService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateSocialLinkRequest> _createValidator;
    private readonly IValidator<UpdateSocialLinkRequest> _updateValidator;

    public SocialLinkService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateSocialLinkRequest> createValidator,
        IValidator<UpdateSocialLinkRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<SocialLinkDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var links = await _dbContext.SocialLinks
            .AsNoTracking()
            .Where(link => link.IsPublished)
            .OrderBy(link => link.DisplayOrder)
            .ThenBy(link => link.Label)
            .ToListAsync(cancellationToken);

        return links.Select(Map).ToArray();
    }

    public async Task<SocialLinkDto> GetPublishedByIdAsync(
        Guid socialLinkId,
        CancellationToken cancellationToken = default)
    {
        var link = await _dbContext.SocialLinks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == socialLinkId && entry.IsPublished,
                cancellationToken);

        if (link is null)
        {
            throw new NotFoundException("SocialLink", socialLinkId);
        }

        return Map(link);
    }

    public async Task<IReadOnlyList<SocialLinkDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var links = await _dbContext.SocialLinks
            .AsNoTracking()
            .Where(link => link.UserId == userId)
            .OrderBy(link => link.DisplayOrder)
            .ThenBy(link => link.Label)
            .ToListAsync(cancellationToken);

        return links.Select(Map).ToArray();
    }

    public async Task<SocialLinkDto> CreateMineAsync(
        CreateSocialLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        SocialLink link;
        try
        {
            link = SocialLink.Create(
                userId,
                request.Platform,
                request.Label,
                request.Url,
                utcNow);
            link.UpdateDetails(
                platform: request.Platform,
                label: request.Label,
                url: request.Url,
                icon: request.Icon,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                link.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.SocialLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "sociallink.create",
            entityName: nameof(SocialLink),
            entityId: link.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(link);
    }

    public async Task<SocialLinkDto> UpdateMineAsync(
        Guid socialLinkId,
        UpdateSocialLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var link = await GetOwnedAsync(userId, socialLinkId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            link.UpdateDetails(
                platform: request.Platform,
                label: request.Label,
                url: request.Url,
                icon: request.Icon,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                link.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                link.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "sociallink.update",
            entityName: nameof(SocialLink),
            entityId: link.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(link);
    }

    public async Task DeleteMineAsync(Guid socialLinkId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var link = await GetOwnedAsync(userId, socialLinkId, cancellationToken);

        _dbContext.SocialLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "sociallink.delete",
            entityName: nameof(SocialLink),
            entityId: link.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<SocialLinkDto> PublishMineAsync(
        Guid socialLinkId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var link = await GetOwnedAsync(userId, socialLinkId, cancellationToken);

        try
        {
            link.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "sociallink.publish",
            entityName: nameof(SocialLink),
            entityId: link.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(link);
    }

    public async Task<SocialLinkDto> UnpublishMineAsync(
        Guid socialLinkId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var link = await GetOwnedAsync(userId, socialLinkId, cancellationToken);

        link.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "sociallink.unpublish",
            entityName: nameof(SocialLink),
            entityId: link.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(link);
    }

    private async Task<SocialLink> GetOwnedAsync(
        string userId,
        Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var link = await _dbContext.SocialLinks
            .SingleOrDefaultAsync(
                entry => entry.Id == socialLinkId && entry.UserId == userId,
                cancellationToken);

        if (link is null)
        {
            throw new NotFoundException("SocialLink", socialLinkId);
        }

        return link;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static SocialLinkDto Map(SocialLink link) =>
        new()
        {
            Id = link.Id,
            Platform = link.Platform,
            Label = link.Label,
            Url = link.Url,
            Icon = link.Icon,
            DisplayOrder = link.DisplayOrder,
            IsPublished = link.IsPublished,
            UpdatedAtUtc = link.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["SocialLink"] = [exception.Message]
        });
}
