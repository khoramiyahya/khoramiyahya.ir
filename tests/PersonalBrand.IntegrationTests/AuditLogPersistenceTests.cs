using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Persistence;
using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class AuditLogPersistenceTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly PersonalBrandWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuditLogPersistenceTests(PersonalBrandWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authenticated_Mutation_Should_Persist_Actor_And_Entity()
    {
        var email = $"audit_{Guid.NewGuid():N}@example.com";
        const string password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Audit User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var auth = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        Assert.NotNull(auth?.Data);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Audited Project",
                slug = $"audit-{Guid.NewGuid():N}"[..20],
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Data.AccessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var project = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        Assert.NotNull(project?.Data);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditLog = await dbContext.AuditLogs
            .AsNoTracking()
            .SingleAsync(entry =>
                entry.Action == "project.create"
                && entry.EntityId == project.Data.Id.ToString());

        Assert.Equal("PortfolioProject", auditLog.EntityName);
        Assert.Equal(auth.Data.User.Id, auditLog.UserId);
        Assert.Null(auditLog.MetadataJson);
        Assert.InRange(auditLog.OccurredOnUtc, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Metadata_Should_Be_Serialized_And_Invalid_Graph_Should_Use_Sentinel()
    {
        var validAction = $"audit.metadata.valid.{Guid.NewGuid():N}";
        var invalidAction = $"audit.metadata.invalid.{Guid.NewGuid():N}";
        var scalarAction = $"audit.metadata.scalar.{Guid.NewGuid():N}";

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
            await auditLogger.LogAsync(validAction, "Sample", "42", new { Changed = true });

            var cyclicMetadata = new CyclicMetadata();
            cyclicMetadata.Self = cyclicMetadata;
            await auditLogger.LogAsync(invalidAction, metadata: cyclicMetadata);
            await auditLogger.LogAsync(scalarAction, metadata: "value");
        }

        await using var assertionScope = _factory.Services.CreateAsyncScope();
        var dbContext = assertionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entries = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(entry => entry.Action == validAction
                || entry.Action == invalidAction
                || entry.Action == scalarAction)
            .ToDictionaryAsync(entry => entry.Action);

        using var metadata = JsonDocument.Parse(entries[validAction].MetadataJson!);
        Assert.True(metadata.RootElement.GetProperty("changed").GetBoolean());
        Assert.Equal("{\"serializationStatus\":\"omitted\"}", entries[invalidAction].MetadataJson);
        Assert.Equal("\"value\"", entries[scalarAction].MetadataJson);
        Assert.Null(entries[validAction].UserId);
    }

    [Fact]
    public async Task PreCanceled_Operation_Should_Not_Persist_Audit_Log()
    {
        var action = $"audit.canceled.{Guid.NewGuid():N}";

        await using var scope = _factory.Services.CreateAsyncScope();
        var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            auditLogger.LogAsync(action, cancellationToken: cancellationTokenSource.Token));

        Assert.False(await dbContext.AuditLogs.AsNoTracking().AnyAsync(entry => entry.Action == action));
    }

    private sealed class CyclicMetadata
    {
        public CyclicMetadata? Self { get; set; }
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }

    private sealed class AuthPayload
    {
        public string AccessToken { get; set; } = string.Empty;

        public UserPayload User { get; set; } = new();
    }

    private sealed class UserPayload
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class ProjectPayload
    {
        public Guid Id { get; set; }
    }
}
