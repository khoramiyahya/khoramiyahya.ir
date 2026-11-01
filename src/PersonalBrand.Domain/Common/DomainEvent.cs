namespace PersonalBrand.Domain.Common;

/// <summary>
/// Convenience base for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}
