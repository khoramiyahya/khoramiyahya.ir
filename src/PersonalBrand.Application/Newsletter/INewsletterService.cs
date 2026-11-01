using PersonalBrand.Application.Newsletter.Contracts;

namespace PersonalBrand.Application.Newsletter;

/// <summary>
/// Newsletter subscription use cases.
/// </summary>
public interface INewsletterService
{
    Task<NewsletterSubscriberDto> SubscribeAsync(
        SubscribeNewsletterRequest request,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(
        UnsubscribeNewsletterRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NewsletterSubscriberDto>> GetSubscribersAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task DeleteSubscriberAsync(Guid subscriberId, CancellationToken cancellationToken = default);
}
