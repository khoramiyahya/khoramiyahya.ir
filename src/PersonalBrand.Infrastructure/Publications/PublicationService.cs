using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Publications;
using PersonalBrand.Application.Publications.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Publications;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Publications;

internal sealed class PublicationService : IPublicationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreatePublicationRequest> _createValidator;
    private readonly IValidator<UpdatePublicationRequest> _updateValidator;

    public PublicationService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreatePublicationRequest> createValidator,
        IValidator<UpdatePublicationRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<PublicationDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var publications = await _dbContext.Publications
            .AsNoTracking()
            .Where(publication => publication.IsPublished)
            .OrderByDescending(publication => publication.IsFeatured)
            .ThenByDescending(publication => publication.PublishedOn)
            .ThenBy(publication => publication.DisplayOrder)
            .ToListAsync(cancellationToken);

        return publications.Select(Map).ToArray();
    }

    public async Task<PublicationDto> GetPublishedByIdAsync(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var publication = await _dbContext.Publications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == publicationId && entry.IsPublished,
                cancellationToken);

        if (publication is null)
        {
            throw new NotFoundException("Publication", publicationId);
        }

        return Map(publication);
    }

    public async Task<IReadOnlyList<PublicationDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var publications = await _dbContext.Publications
            .AsNoTracking()
            .Where(publication => publication.UserId == userId)
            .OrderByDescending(publication => publication.PublishedOn)
            .ThenBy(publication => publication.DisplayOrder)
            .ToListAsync(cancellationToken);

        return publications.Select(Map).ToArray();
    }

    public async Task<PublicationDto> CreateMineAsync(
        CreatePublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Publication publication;
        try
        {
            publication = Publication.Create(
                userId,
                request.Title,
                request.Publisher,
                request.PublishedOn,
                utcNow);
            publication.UpdateDetails(
                title: request.Title,
                publisher: request.Publisher,
                publicationType: request.PublicationType,
                description: request.Description,
                url: request.Url,
                publishedOn: request.PublishedOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                publication.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Publications.Add(publication);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "publication.create",
            entityName: nameof(Publication),
            entityId: publication.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(publication);
    }

    public async Task<PublicationDto> UpdateMineAsync(
        Guid publicationId,
        UpdatePublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var publication = await GetOwnedAsync(userId, publicationId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            publication.UpdateDetails(
                title: request.Title,
                publisher: request.Publisher,
                publicationType: request.PublicationType,
                description: request.Description,
                url: request.Url,
                publishedOn: request.PublishedOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                publication.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                publication.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "publication.update",
            entityName: nameof(Publication),
            entityId: publication.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(publication);
    }

    public async Task DeleteMineAsync(Guid publicationId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var publication = await GetOwnedAsync(userId, publicationId, cancellationToken);

        _dbContext.Publications.Remove(publication);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "publication.delete",
            entityName: nameof(Publication),
            entityId: publication.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<PublicationDto> PublishMineAsync(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var publication = await GetOwnedAsync(userId, publicationId, cancellationToken);

        try
        {
            publication.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "publication.publish",
            entityName: nameof(Publication),
            entityId: publication.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(publication);
    }

    public async Task<PublicationDto> UnpublishMineAsync(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var publication = await GetOwnedAsync(userId, publicationId, cancellationToken);

        publication.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "publication.unpublish",
            entityName: nameof(Publication),
            entityId: publication.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(publication);
    }

    private async Task<Publication> GetOwnedAsync(
        string userId,
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var publication = await _dbContext.Publications
            .SingleOrDefaultAsync(
                entry => entry.Id == publicationId && entry.UserId == userId,
                cancellationToken);

        if (publication is null)
        {
            throw new NotFoundException("Publication", publicationId);
        }

        return publication;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static PublicationDto Map(Publication publication) =>
        new()
        {
            Id = publication.Id,
            Title = publication.Title,
            Publisher = publication.Publisher,
            PublicationType = publication.PublicationType,
            Description = publication.Description,
            Url = publication.Url,
            PublishedOn = publication.PublishedOn,
            DisplayOrder = publication.DisplayOrder,
            IsFeatured = publication.IsFeatured,
            IsPublished = publication.IsPublished,
            UpdatedAtUtc = publication.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Publication"] = [exception.Message]
        });
}
