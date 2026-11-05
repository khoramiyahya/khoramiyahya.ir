namespace PersonalBrand.Infrastructure.Auditing;

/// <summary>
/// Durable record of a security-sensitive or business-significant action.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; }

    public required string Action { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? UserId { get; set; }

    public DateTimeOffset OccurredOnUtc { get; set; }

    public string? MetadataJson { get; set; }
}
