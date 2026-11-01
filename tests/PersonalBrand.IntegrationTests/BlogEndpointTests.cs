using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class BlogEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public BlogEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Publish_List_And_Delete_BlogPost_Should_Succeed()
    {
        var email = $"blog_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Blog Author"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        var slug = $"hello-blog-{Guid.NewGuid():N}"[..24];

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/blog/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Hello Blog",
                slug,
                summary = "First post on the personal brand platform.",
                body = "This is the full article body used for publishing.",
                tags = "dotnet,branding",
                isFeatured = true,
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<BlogPayload>>(JsonOptions);
        Assert.NotNull(createPayload?.Data);
        Assert.True(createPayload.Data.IsPublished);
        Assert.Equal("Hello Blog", createPayload.Data.Title);

        var publicListResponse = await _client.GetAsync("/api/v1/blog/public");
        Assert.Equal(HttpStatusCode.OK, publicListResponse.StatusCode);
        var publicList = await publicListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<BlogPayload>>>(JsonOptions);
        Assert.Contains(publicList!.Data!, item => item.Slug == slug);

        var publicItemResponse = await _client.GetAsync($"/api/v1/blog/public/{slug}");
        Assert.Equal(HttpStatusCode.OK, publicItemResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/blog/me/{createPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/v1/blog/public/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Unpublished_BlogPost_Should_Not_Appear_In_Public_List()
    {
        var email = $"blog_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft Author"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;
        var slug = $"draft-blog-{Guid.NewGuid():N}"[..24];

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/blog/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Draft Blog Post",
                slug,
                body = "Draft body content.",
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var publicResponse = await _client.GetAsync($"/api/v1/blog/public/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, publicResponse.StatusCode);
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

    private sealed class BlogPayload
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
    }
}
