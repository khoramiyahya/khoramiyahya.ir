namespace PersonalBrand.Application.Abstractions;

/// <summary>
/// Application-level audit logging abstraction. Persistence strategy is decided by Infrastructure.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
