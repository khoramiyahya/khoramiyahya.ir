using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Persistence;
using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class AuditTransactionTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public async Task Audit_Failure_Should_Roll_Back_Business_Mutation()
    {
        await using var factory = new PersonalBrandWebApplicationFactory(services =>
        {
            services.RemoveAll<IAuditLogger>();
            services.AddScoped<IAuditLogger, FailingAuditLogger>();
        });
        using var client = factory.CreateClient();
        var email = $"rollback_{Guid.NewGuid():N}@example.com";
        const string password = "StrongPass1!";

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Rollback User"
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await dbContext.Users.AsNoTracking().AnyAsync(user => user.Email == email));
        Assert.False(await dbContext.RefreshTokens.AsNoTracking().AnyAsync());
    }

    [Fact]
    public async Task Audit_Failure_Should_Roll_Back_File_And_Database_Mutations()
    {
        await using var factory = new PersonalBrandWebApplicationFactory(services =>
        {
            services.RemoveAll<IAuditLogger>();
            services.AddScoped<IAuditLogger, CoverUploadFailingAuditLogger>();
        });
        using var client = factory.CreateClient();
        var email = $"file_rollback_{Guid.NewGuid():N}@example.com";
        var fileName = $"rollback_{Guid.NewGuid():N}.png";
        const string password = "StrongPass1!";

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "File Rollback User"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var auth = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        Assert.NotNull(auth?.Data);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = "File Rollback Project",
                slug = $"rollback-{Guid.NewGuid():N}"[..20],
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Data.AccessToken);
        var createResponse = await client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var project = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        Assert.NotNull(project?.Data);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TinyPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", fileName);
        using var uploadRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/projects/me/{project.Data.Id}/cover")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Data.AccessToken);

        var uploadResponse = await client.SendAsync(uploadRequest);

        Assert.Equal(HttpStatusCode.InternalServerError, uploadResponse.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storedProject = await dbContext.PortfolioProjects
            .AsNoTracking()
            .SingleAsync(item => item.Id == project.Data.Id);
        Assert.Null(storedProject.CoverImagePath);

        var matchingFiles = Directory.Exists(factory.FileStorageRootPath)
            ? Directory.EnumerateFiles(factory.FileStorageRootPath, $"*{fileName}", SearchOption.AllDirectories)
            : [];
        Assert.Empty(matchingFiles);

        var transactionRoot = Path.Combine(
            Path.GetDirectoryName(factory.FileStorageRootPath)!,
            $".{Path.GetFileName(factory.FileStorageRootPath)}.transactions");
        Assert.False(Directory.Exists(transactionRoot));
    }

    [Fact]
    public async Task Uncertain_Commit_Should_Preserve_Old_And_New_Files()
    {
        await using var factory = new PersonalBrandWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var fileTransaction = scope.ServiceProvider.GetRequiredService<IFileStorageTransaction>();
        string? oldPath = null;
        string? newPath = null;

        try
        {
            await using var oldContent = new MemoryStream("old"u8.ToArray());
            oldPath = await fileStorage.SaveAsync(oldContent, "old.txt", "text/plain");

            fileTransaction.Begin();
            await using var newContent = new MemoryStream("new"u8.ToArray());
            newPath = await fileStorage.SaveAsync(newContent, "new.txt", "text/plain");
            await fileStorage.DeleteAsync(oldPath);
            await fileTransaction.PrepareAsync();

            await fileTransaction.AbandonAsync();

            Assert.True(File.Exists(ToAbsolutePath(factory, oldPath)));
            Assert.True(File.Exists(ToAbsolutePath(factory, newPath)));
        }
        finally
        {
            if (oldPath is not null)
            {
                await fileStorage.DeleteAsync(oldPath);
            }

            if (newPath is not null)
            {
                await fileStorage.DeleteAsync(newPath);
            }
        }
    }

    private sealed class FailingAuditLogger : IAuditLogger
    {
        public Task LogAsync(
            string action,
            string? entityName = null,
            string? entityId = null,
            object? metadata = null,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated durable audit failure.");
    }

    private sealed class CoverUploadFailingAuditLogger : IAuditLogger
    {
        public Task LogAsync(
            string action,
            string? entityName = null,
            string? entityId = null,
            object? metadata = null,
            CancellationToken cancellationToken = default) =>
            action == "project.cover.upload"
                ? throw new InvalidOperationException("Simulated file audit failure.")
                : Task.CompletedTask;
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }

    private sealed class AuthPayload
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class ProjectPayload
    {
        public Guid Id { get; set; }
    }

    private static string ToAbsolutePath(
        PersonalBrandWebApplicationFactory factory,
        string relativePath) =>
        Path.Combine(
            factory.FileStorageRootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
}
