using System.Text.Json;

using Microsoft.Extensions.Logging;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Auditing;

internal sealed class DatabaseAuditLogger : IAuditLogger
{
    internal const string OmittedMetadataJson = "{\"serializationStatus\":\"omitted\"}";

    private const int MaxMetadataLength = 4000;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        MaxDepth = 16
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<DatabaseAuditLogger> _logger;

    public DatabaseAuditLogger(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        ILogger<DatabaseAuditLogger> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        cancellationToken.ThrowIfCancellationRequested();

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null,
            OccurredOnUtc = _dateTimeProvider.UtcNow,
            MetadataJson = SerializeMetadata(action, metadata)
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private string? SerializeMetadata(string action, object? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(metadata, metadata.GetType(), SerializerOptions);
            if (json.Length <= MaxMetadataLength)
            {
                return json;
            }

            _logger.LogWarning(
                "Audit metadata for action {Action} was omitted because it exceeded {MaxLength} characters.",
                action,
                MaxMetadataLength);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            _logger.LogWarning(
                exception,
                "Audit metadata for action {Action} was omitted because serialization failed.",
                action);
        }

        return OmittedMetadataJson;
    }
}
