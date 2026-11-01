using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class ProjectCoverUploadTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 1x1 transparent PNG
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly HttpClient _client;

    public ProjectCoverUploadTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_And_Remove_Cover_Should_Succeed()
    {
        var email = $"cover_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Cover User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        var slug = $"cover-{Guid.NewGuid():N}"[..20];
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Cover Project",
                slug,
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        var projectId = createPayload!.Data!.Id;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TinyPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "cover.png");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/projects/me/{projectId}/cover")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload?.Data?.CoverImagePath));
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload?.Data?.CoverImageUrl));
        Assert.StartsWith("/uploads/", uploadPayload!.Data!.CoverImageUrl);

        var fileResponse = await _client.GetAsync(uploadPayload.Data.CoverImageUrl);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/projects/me/{projectId}/cover");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        Assert.Null(deletePayload?.Data?.CoverImagePath);
        Assert.Null(deletePayload?.Data?.CoverImageUrl);
    }

    [Fact]
    public async Task Upload_Invalid_Content_Type_Should_Fail()
    {
        var email = $"cover_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Cover User"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        var slug = $"badcover-{Guid.NewGuid():N}"[..20];
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Bad Cover Project",
                slug,
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProjectPayload>>(JsonOptions);
        var projectId = createPayload!.Data!.Id;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("not-an-image"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "notes.txt");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/projects/me/{projectId}/cover")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.BadRequest, uploadResponse.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }
    }

    private sealed class AuthPayload
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class ProjectPayload
    {
        public Guid Id { get; set; }

        public string? CoverImagePath { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
