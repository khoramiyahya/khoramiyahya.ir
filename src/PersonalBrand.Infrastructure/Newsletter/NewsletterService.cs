using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Newsletter;
using PersonalBrand.Application.Newsletter.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Newsletter;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Newsletter;

internal sealed class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<SubscribeNewsletterRequest> _subscribeValidator;
    private readonly IValidator<UnsubscribeNewsletterRequest> _unsubscribeValidator;

    public NewsletterService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<SubscribeNewsletterRequest> subscribeValidator,
        IValidator<UnsubscribeNewsletterRequest> unsubscribeValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _subscribeValidator = subscribeValidator;
        _unsubscribeValidator = unsubscribeValidator;
    }

    public async Task<NewsletterSubscriberDto> SubscribeAsync(
        SubscribeNewsletterRequest request,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        await _subscribeValidator.EnsureValidAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _dbContext.NewsletterSubscribers
            .SingleOrDefaultAsync(subscriber => subscriber.Email == normalizedEmail, cancellationToken);

        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            if (existing is null)
            {
                var subscriber = NewsletterSubscriber.Create(
                    request.Email,
                    utcNow,
                    request.Name,
                    clientIp,
                    userAgent);

                _dbContext.NewsletterSubscribers.Add(subscriber);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _auditLogger.LogAsync(
                    action: "newsletter.subscribe",
                    entityName: nameof(NewsletterSubscriber),
                    entityId: subscriber.Id.ToString(),
                    cancellationToken: cancellationToken);

                return Map(subscriber);
            }

            if (existing.IsActive)
            {
                throw new ConflictException("This email is already subscribed.");
            }

            existing.Resubscribe(utcNow, request.Name, clientIp, userAgent);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogAsync(
                action: "newsletter.resubscribe",
                entityName: nameof(NewsletterSubscriber),
                entityId: existing.Id.ToString(),
                cancellationToken: cancellationToken);

            return Map(existing);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }
    }

    public async Task UnsubscribeAsync(
        UnsubscribeNewsletterRequest request,
        CancellationToken cancellationToken = default)
    {
        await _unsubscribeValidator.EnsureValidAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _dbContext.NewsletterSubscribers
            .SingleOrDefaultAsync(subscriber => subscriber.Email == normalizedEmail, cancellationToken);

        if (existing is null)
        {
            // Avoid email enumeration; treat as successful no-op.
            return;
        }

        existing.Unsubscribe(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "newsletter.unsubscribe",
            entityName: nameof(NewsletterSubscriber),
            entityId: existing.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<NewsletterSubscriberDto>> GetSubscribersAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();

        var query = _dbContext.NewsletterSubscribers.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(subscriber => subscriber.IsActive);
        }

        var subscribers = await query
            .OrderByDescending(subscriber => subscriber.SubscribedAtUtc)
            .ToListAsync(cancellationToken);

        return subscribers.Select(Map).ToArray();
    }

    public async Task DeleteSubscriberAsync(Guid subscriberId, CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();

        var subscriber = await _dbContext.NewsletterSubscribers
            .SingleOrDefaultAsync(item => item.Id == subscriberId, cancellationToken);

        if (subscriber is null)
        {
            throw new NotFoundException("NewsletterSubscriber", subscriberId);
        }

        _dbContext.NewsletterSubscribers.Remove(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "newsletter.delete",
            entityName: nameof(NewsletterSubscriber),
            entityId: subscriber.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    private void RequireAuthenticatedUser()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }
    }

    private static NewsletterSubscriberDto Map(NewsletterSubscriber subscriber) =>
        new()
        {
            Id = subscriber.Id,
            Email = subscriber.Email,
            Name = subscriber.Name,
            IsActive = subscriber.IsActive,
            SubscribedAtUtc = subscriber.SubscribedAtUtc,
            UnsubscribedAtUtc = subscriber.UnsubscribedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Newsletter"] = [exception.Message]
        });
}
