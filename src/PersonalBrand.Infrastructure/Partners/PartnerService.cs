using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Partners;
using PersonalBrand.Application.Partners.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Partners;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Partners;

internal sealed class PartnerService : IPartnerService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreatePartnerRequest> _createValidator;
    private readonly IValidator<UpdatePartnerRequest> _updateValidator;

    public PartnerService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreatePartnerRequest> createValidator,
        IValidator<UpdatePartnerRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<PartnerDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var partners = await _dbContext.Partners
            .AsNoTracking()
            .Where(partner => partner.IsPublished)
            .OrderByDescending(partner => partner.IsFeatured)
            .ThenBy(partner => partner.DisplayOrder)
            .ThenBy(partner => partner.Name)
            .ToListAsync(cancellationToken);

        return partners.Select(Map).ToArray();
    }

    public async Task<PartnerDto> GetPublishedByIdAsync(
        Guid partnerId,
        CancellationToken cancellationToken = default)
    {
        var partner = await _dbContext.Partners
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == partnerId && entry.IsPublished,
                cancellationToken);

        if (partner is null)
        {
            throw new NotFoundException("Partner", partnerId);
        }

        return Map(partner);
    }

    public async Task<IReadOnlyList<PartnerDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var partners = await _dbContext.Partners
            .AsNoTracking()
            .Where(partner => partner.UserId == userId)
            .OrderBy(partner => partner.DisplayOrder)
            .ThenBy(partner => partner.Name)
            .ToListAsync(cancellationToken);

        return partners.Select(Map).ToArray();
    }

    public async Task<PartnerDto> CreateMineAsync(
        CreatePartnerRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Partner partner;
        try
        {
            partner = Partner.Create(userId, request.Name, utcNow);
            partner.UpdateDetails(
                name: request.Name,
                websiteUrl: request.WebsiteUrl,
                logoUrl: request.LogoUrl,
                summary: request.Summary,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                partner.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Partners.Add(partner);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "partner.create",
            entityName: nameof(Partner),
            entityId: partner.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(partner);
    }

    public async Task<PartnerDto> UpdateMineAsync(
        Guid partnerId,
        UpdatePartnerRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var partner = await GetOwnedAsync(userId, partnerId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            partner.UpdateDetails(
                name: request.Name,
                websiteUrl: request.WebsiteUrl,
                logoUrl: request.LogoUrl,
                summary: request.Summary,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                partner.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                partner.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "partner.update",
            entityName: nameof(Partner),
            entityId: partner.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(partner);
    }

    public async Task DeleteMineAsync(Guid partnerId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var partner = await GetOwnedAsync(userId, partnerId, cancellationToken);

        _dbContext.Partners.Remove(partner);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "partner.delete",
            entityName: nameof(Partner),
            entityId: partner.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<PartnerDto> PublishMineAsync(
        Guid partnerId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var partner = await GetOwnedAsync(userId, partnerId, cancellationToken);

        try
        {
            partner.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "partner.publish",
            entityName: nameof(Partner),
            entityId: partner.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(partner);
    }

    public async Task<PartnerDto> UnpublishMineAsync(
        Guid partnerId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var partner = await GetOwnedAsync(userId, partnerId, cancellationToken);

        partner.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "partner.unpublish",
            entityName: nameof(Partner),
            entityId: partner.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(partner);
    }

    private async Task<Partner> GetOwnedAsync(
        string userId,
        Guid partnerId,
        CancellationToken cancellationToken)
    {
        var partner = await _dbContext.Partners
            .SingleOrDefaultAsync(
                entry => entry.Id == partnerId && entry.UserId == userId,
                cancellationToken);

        if (partner is null)
        {
            throw new NotFoundException("Partner", partnerId);
        }

        return partner;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static PartnerDto Map(Partner partner) =>
        new()
        {
            Id = partner.Id,
            Name = partner.Name,
            WebsiteUrl = partner.WebsiteUrl,
            LogoUrl = partner.LogoUrl,
            Summary = partner.Summary,
            DisplayOrder = partner.DisplayOrder,
            IsFeatured = partner.IsFeatured,
            IsPublished = partner.IsPublished,
            UpdatedAtUtc = partner.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Partner"] = [exception.Message]
        });
}
