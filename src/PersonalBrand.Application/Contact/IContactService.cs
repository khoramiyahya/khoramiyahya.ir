using PersonalBrand.Application.Contact.Contracts;

namespace PersonalBrand.Application.Contact;

/// <summary>
/// Contact inbox use cases.
/// </summary>
public interface IContactService
{
    Task<ContactMessageDto> SubmitAsync(
        SubmitContactRequest request,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactMessageDto>> GetInboxAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkReadAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkUnreadAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid messageId, CancellationToken cancellationToken = default);
}
