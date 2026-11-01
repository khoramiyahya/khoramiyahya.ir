using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Contact;
using PersonalBrand.Application.Contact.Contracts;
using PersonalBrand.Domain.Contact;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Contact;

internal sealed class ContactService : IContactService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<SubmitContactRequest> _submitValidator;

    public ContactService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<SubmitContactRequest> submitValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _submitValidator = submitValidator;
    }

    public async Task<ContactMessageDto> SubmitAsync(
        SubmitContactRequest request,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        await _submitValidator.EnsureValidAsync(request, cancellationToken);

        ContactMessage message;
        try
        {
            message = ContactMessage.Create(
                request.Name,
                request.Email,
                request.Subject,
                request.Body,
                _dateTimeProvider.UtcNow,
                request.Phone,
                clientIp,
                userAgent);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.ContactMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "contact.submit",
            entityName: nameof(ContactMessage),
            entityId: message.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(message);
    }

    public async Task<IReadOnlyList<ContactMessageDto>> GetInboxAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();

        var query = _dbContext.ContactMessages.AsNoTracking();
        if (unreadOnly)
        {
            query = query.Where(message => !message.IsRead);
        }

        var messages = await query
            .OrderByDescending(message => message.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return messages.Select(Map).ToArray();
    }

    public async Task<ContactMessageDto> GetByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        var message = await GetRequiredAsync(messageId, tracking: false, cancellationToken);
        return Map(message);
    }

    public async Task<ContactMessageDto> MarkReadAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        var message = await GetRequiredAsync(messageId, tracking: true, cancellationToken);

        message.MarkRead(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "contact.read",
            entityName: nameof(ContactMessage),
            entityId: message.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(message);
    }

    public async Task<ContactMessageDto> MarkUnreadAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        var message = await GetRequiredAsync(messageId, tracking: true, cancellationToken);

        message.MarkUnread();
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "contact.unread",
            entityName: nameof(ContactMessage),
            entityId: message.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(message);
    }

    public async Task DeleteAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        var message = await GetRequiredAsync(messageId, tracking: true, cancellationToken);

        _dbContext.ContactMessages.Remove(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "contact.delete",
            entityName: nameof(ContactMessage),
            entityId: message.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    private async Task<ContactMessage> GetRequiredAsync(
        Guid messageId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = tracking
            ? _dbContext.ContactMessages.AsQueryable()
            : _dbContext.ContactMessages.AsNoTracking();

        var message = await query.SingleOrDefaultAsync(item => item.Id == messageId, cancellationToken);
        if (message is null)
        {
            throw new NotFoundException("ContactMessage", messageId);
        }

        return message;
    }

    private void RequireAuthenticatedUser()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }
    }

    private static ContactMessageDto Map(ContactMessage message) =>
        new()
        {
            Id = message.Id,
            Name = message.Name,
            Email = message.Email,
            Phone = message.Phone,
            Subject = message.Subject,
            Body = message.Body,
            IsRead = message.IsRead,
            ReadAtUtc = message.ReadAtUtc,
            CreatedAtUtc = message.CreatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Contact"] = [exception.Message]
        });
}
