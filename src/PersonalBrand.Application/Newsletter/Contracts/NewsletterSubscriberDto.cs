namespace PersonalBrand.Application.Newsletter.Contracts;

/// <summary>
/// Owner representation of a newsletter subscriber.
/// </summary>
public sealed class NewsletterSubscriberDto
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public string? Name { get; init; }

    public required bool IsActive { get; init; }

    public required DateTimeOffset SubscribedAtUtc { get; init; }

    public DateTimeOffset? UnsubscribedAtUtc { get; init; }
}
