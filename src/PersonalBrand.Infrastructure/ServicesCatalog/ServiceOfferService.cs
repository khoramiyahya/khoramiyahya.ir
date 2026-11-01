using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Services;
using PersonalBrand.Application.Services.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Domain.Services;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.ServicesCatalog;

internal sealed class ServiceOfferService : IServiceOfferService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateServiceOfferRequest> _createValidator;
    private readonly IValidator<UpdateServiceOfferRequest> _updateValidator;

    public ServiceOfferService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateServiceOfferRequest> createValidator,
        IValidator<UpdateServiceOfferRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ServiceOfferDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var services = await _dbContext.ServiceOffers
            .AsNoTracking()
            .Where(service => service.IsPublished)
            .OrderByDescending(service => service.IsFeatured)
            .ThenBy(service => service.DisplayOrder)
            .ThenByDescending(service => service.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return services.Select(Map).ToArray();
    }

    public async Task<ServiceOfferDto> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException("Published service was not found.");
        }

        string normalizedSlug;
        try
        {
            normalizedSlug = ProfileSlug.Normalize(slug);
        }
        catch (DomainException)
        {
            throw new NotFoundException("Published service was not found.");
        }

        var service = await _dbContext.ServiceOffers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug && item.IsPublished,
                cancellationToken);

        if (service is null)
        {
            throw new NotFoundException("ServiceOffer", normalizedSlug);
        }

        return Map(service);
    }

    public async Task<IReadOnlyList<ServiceOfferDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var services = await _dbContext.ServiceOffers
            .AsNoTracking()
            .Where(service => service.UserId == userId)
            .OrderBy(service => service.DisplayOrder)
            .ThenByDescending(service => service.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return services.Select(Map).ToArray();
    }

    public async Task<ServiceOfferDto> CreateMineAsync(
        CreateServiceOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        ServiceOffer service;
        try
        {
            service = ServiceOffer.Create(userId, request.Title, utcNow, request.Slug);
            await EnsureUniqueSlugAsync(service, cancellationToken);

            service.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                description: request.Description,
                priceLabel: request.PriceLabel,
                iconName: request.IconName,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                slug: null,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                service.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.ServiceOffers.Add(service);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "service.create",
            entityName: nameof(ServiceOffer),
            entityId: service.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(service);
    }

    public async Task<ServiceOfferDto> UpdateMineAsync(
        Guid serviceId,
        UpdateServiceOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var service = await GetOwnedServiceAsync(userId, serviceId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? service.Slug
                : ProfileSlug.Normalize(request.Slug);

            await EnsureSlugIsAvailableAsync(desiredSlug, service.Id, cancellationToken);

            service.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                description: request.Description,
                priceLabel: request.PriceLabel,
                iconName: request.IconName,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                slug: desiredSlug,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                service.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                service.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "service.update",
            entityName: nameof(ServiceOffer),
            entityId: service.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(service);
    }

    public async Task DeleteMineAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var service = await GetOwnedServiceAsync(userId, serviceId, cancellationToken);

        _dbContext.ServiceOffers.Remove(service);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "service.delete",
            entityName: nameof(ServiceOffer),
            entityId: service.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<ServiceOfferDto> PublishMineAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var service = await GetOwnedServiceAsync(userId, serviceId, cancellationToken);

        try
        {
            service.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "service.publish",
            entityName: nameof(ServiceOffer),
            entityId: service.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(service);
    }

    public async Task<ServiceOfferDto> UnpublishMineAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var service = await GetOwnedServiceAsync(userId, serviceId, cancellationToken);

        service.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "service.unpublish",
            entityName: nameof(ServiceOffer),
            entityId: service.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(service);
    }

    private async Task EnsureUniqueSlugAsync(ServiceOffer service, CancellationToken cancellationToken)
    {
        var baseSlug = service.Slug;
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.ServiceOffers.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        if (!string.Equals(candidate, service.Slug, StringComparison.Ordinal))
        {
            service.EnsureSlug(candidate, _dateTimeProvider.UtcNow);
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid currentServiceId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ServiceOffers
            .AnyAsync(
                service => service.Slug == slug && service.Id != currentServiceId,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Slug '{slug}' is already in use.");
        }
    }

    private async Task<ServiceOffer> GetOwnedServiceAsync(
        string userId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var service = await _dbContext.ServiceOffers
            .SingleOrDefaultAsync(
                item => item.Id == serviceId && item.UserId == userId,
                cancellationToken);

        if (service is null)
        {
            throw new NotFoundException("ServiceOffer", serviceId);
        }

        return service;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static ServiceOfferDto Map(ServiceOffer service) =>
        new()
        {
            Id = service.Id,
            Slug = service.Slug,
            Title = service.Title,
            Summary = service.Summary,
            Description = service.Description,
            PriceLabel = service.PriceLabel,
            IconName = service.IconName,
            DisplayOrder = service.DisplayOrder,
            IsFeatured = service.IsFeatured,
            IsPublished = service.IsPublished,
            UpdatedAtUtc = service.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Service"] = [exception.Message]
        });
}
