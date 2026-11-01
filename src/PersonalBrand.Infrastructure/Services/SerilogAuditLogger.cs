using Microsoft.Extensions.Logging;

using PersonalBrand.Application.Abstractions;

namespace PersonalBrand.Infrastructure.Services;

/// <summary>
/// Temporary audit logger that writes structured audit events to the application log.
/// Replace with durable storage when the audit module is implemented.
/// </summary>
internal sealed class SerilogAuditLogger : IAuditLogger
{
    private readonly ILogger<SerilogAuditLogger> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SerilogAuditLogger(
        ILogger<SerilogAuditLogger> logger,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Audit event {Action} Entity={EntityName}/{EntityId} User={UserId} At={OccurredOnUtc} Metadata={@Metadata}",
            action,
            entityName,
            entityId,
            _currentUserService.UserId,
            _dateTimeProvider.UtcNow,
            metadata);

        return Task.CompletedTask;
    }
}
