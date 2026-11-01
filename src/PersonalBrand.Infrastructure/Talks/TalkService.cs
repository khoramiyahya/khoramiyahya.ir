using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Talks;
using PersonalBrand.Application.Talks.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Talks;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Talks;

internal sealed class TalkService : ITalkService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateTalkRequest> _createValidator;
    private readonly IValidator<UpdateTalkRequest> _updateValidator;

    public TalkService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateTalkRequest> createValidator,
        IValidator<UpdateTalkRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TalkDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var talks = await _dbContext.Talks
            .AsNoTracking()
            .Where(talk => talk.IsPublished)
            .OrderByDescending(talk => talk.IsFeatured)
            .ThenByDescending(talk => talk.SpokenOn)
            .ThenBy(talk => talk.DisplayOrder)
            .ToListAsync(cancellationToken);

        return talks.Select(Map).ToArray();
    }

    public async Task<TalkDto> GetPublishedByIdAsync(
        Guid talkId,
        CancellationToken cancellationToken = default)
    {
        var talk = await _dbContext.Talks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == talkId && entry.IsPublished,
                cancellationToken);

        if (talk is null)
        {
            throw new NotFoundException("Talk", talkId);
        }

        return Map(talk);
    }

    public async Task<IReadOnlyList<TalkDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var talks = await _dbContext.Talks
            .AsNoTracking()
            .Where(talk => talk.UserId == userId)
            .OrderByDescending(talk => talk.SpokenOn)
            .ThenBy(talk => talk.DisplayOrder)
            .ToListAsync(cancellationToken);

        return talks.Select(Map).ToArray();
    }

    public async Task<TalkDto> CreateMineAsync(
        CreateTalkRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Talk talk;
        try
        {
            talk = Talk.Create(userId, request.Title, request.EventName, request.SpokenOn, utcNow);
            talk.UpdateDetails(
                title: request.Title,
                eventName: request.EventName,
                location: request.Location,
                description: request.Description,
                url: request.Url,
                spokenOn: request.SpokenOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                talk.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Talks.Add(talk);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "talk.create",
            entityName: nameof(Talk),
            entityId: talk.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(talk);
    }

    public async Task<TalkDto> UpdateMineAsync(
        Guid talkId,
        UpdateTalkRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var talk = await GetOwnedAsync(userId, talkId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            talk.UpdateDetails(
                title: request.Title,
                eventName: request.EventName,
                location: request.Location,
                description: request.Description,
                url: request.Url,
                spokenOn: request.SpokenOn,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                talk.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                talk.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "talk.update",
            entityName: nameof(Talk),
            entityId: talk.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(talk);
    }

    public async Task DeleteMineAsync(Guid talkId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var talk = await GetOwnedAsync(userId, talkId, cancellationToken);

        _dbContext.Talks.Remove(talk);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "talk.delete",
            entityName: nameof(Talk),
            entityId: talk.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<TalkDto> PublishMineAsync(
        Guid talkId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var talk = await GetOwnedAsync(userId, talkId, cancellationToken);

        try
        {
            talk.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "talk.publish",
            entityName: nameof(Talk),
            entityId: talk.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(talk);
    }

    public async Task<TalkDto> UnpublishMineAsync(
        Guid talkId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var talk = await GetOwnedAsync(userId, talkId, cancellationToken);

        talk.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "talk.unpublish",
            entityName: nameof(Talk),
            entityId: talk.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(talk);
    }

    private async Task<Talk> GetOwnedAsync(
        string userId,
        Guid talkId,
        CancellationToken cancellationToken)
    {
        var talk = await _dbContext.Talks
            .SingleOrDefaultAsync(
                entry => entry.Id == talkId && entry.UserId == userId,
                cancellationToken);

        if (talk is null)
        {
            throw new NotFoundException("Talk", talkId);
        }

        return talk;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static TalkDto Map(Talk talk) =>
        new()
        {
            Id = talk.Id,
            Title = talk.Title,
            EventName = talk.EventName,
            Location = talk.Location,
            Description = talk.Description,
            Url = talk.Url,
            SpokenOn = talk.SpokenOn,
            DisplayOrder = talk.DisplayOrder,
            IsFeatured = talk.IsFeatured,
            IsPublished = talk.IsPublished,
            UpdatedAtUtc = talk.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Talk"] = [exception.Message]
        });
}
