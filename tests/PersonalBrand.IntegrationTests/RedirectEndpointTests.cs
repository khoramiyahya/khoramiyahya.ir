using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class RedirectEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public RedirectEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Publish_Resolve_And_Delete_Redirect_Should_Succeed()
    {
        var email = $"redirect_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var fromPath = $"/old-page-{Guid.NewGuid():N}"[..24];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Redirect Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/redirects/me")
        {
            Content = JsonContent.Create(new
            {
                fromPath,
                toUrl = "/about",
                statusCode = 301,
                notes = "Moved about page",
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<RedirectPayload>>(JsonOptions);
        Assert.NotNull(createPayload?.Data);
        Assert.True(createPayload.Data.IsPublished);
        Assert.Equal(fromPath, createPayload.Data.FromPath);
        Assert.Equal(301, createPayload.Data.StatusCode);

        var publicListResponse = await _client.GetAsync("/api/v1/redirects/public");
        Assert.Equal(HttpStatusCode.OK, publicListResponse.StatusCode);
        var publicList = await publicListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<RedirectPayload>>>(JsonOptions);
        Assert.Contains(publicList!.Data!, item => item.Id == createPayload.Data.Id);

        var resolveResponse = await _client.GetAsync($"/api/v1/redirects/public/resolve?fromPath={Uri.EscapeDataString(fromPath)}");
        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolvePayload = await resolveResponse.Content.ReadFromJsonAsync<ApiEnvelope<RedirectPayload>>(JsonOptions);
        Assert.Equal("/about", resolvePayload!.Data!.ToUrl);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/redirects/me/{createPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/v1/redirects/public/resolve?fromPath={Uri.EscapeDataString(fromPath)}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Unpublished_Redirect_Should_Not_Resolve_Publicly()
    {
        var email = $"redirect_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var fromPath = $"/draft-redirect-{Guid.NewGuid():N}"[..28];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft Redirect Owner"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/redirects/me")
        {
            Content = JsonContent.Create(new
            {
                fromPath,
                toUrl = "/home",
                statusCode = 302,
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var resolveResponse = await _client.GetAsync($"/api/v1/redirects/public/resolve?fromPath={Uri.EscapeDataString(fromPath)}");
        Assert.Equal(HttpStatusCode.NotFound, resolveResponse.StatusCode);
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

    private sealed class RedirectPayload
    {
        public Guid Id { get; set; }

        public string FromPath { get; set; } = string.Empty;

        public string ToUrl { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public bool IsPublished { get; set; }
    }
}
