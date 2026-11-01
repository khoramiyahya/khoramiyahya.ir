using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class LanguageEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public LanguageEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Publish_List_And_Delete_Language_Should_Succeed()
    {
        var email = $"language_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var name = $"Persian-{Guid.NewGuid():N}"[..16];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Language Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/languages/me")
        {
            Content = JsonContent.Create(new
            {
                name,
                proficiency = "native",
                displayOrder = 1,
                isFeatured = true,
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<LanguagePayload>>(JsonOptions);
        Assert.NotNull(createPayload?.Data);
        Assert.True(createPayload.Data.IsPublished);
        Assert.Equal(name, createPayload.Data.Name);

        var publicListResponse = await _client.GetAsync("/api/v1/languages/public");
        Assert.Equal(HttpStatusCode.OK, publicListResponse.StatusCode);
        var publicList = await publicListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<LanguagePayload>>>(JsonOptions);
        Assert.Contains(publicList!.Data!, item => item.Id == createPayload.Data.Id);

        var publicItemResponse = await _client.GetAsync($"/api/v1/languages/public/{createPayload.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, publicItemResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/languages/me/{createPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/v1/languages/public/{createPayload.Data.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Unpublished_Language_Should_Not_Appear_In_Public_List()
    {
        var email = $"language_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft Language Owner"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/languages/me")
        {
            Content = JsonContent.Create(new
            {
                name = $"English-{Guid.NewGuid():N}"[..16],
                proficiency = "fluent",
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<LanguagePayload>>(JsonOptions);

        var publicResponse = await _client.GetAsync($"/api/v1/languages/public/{createPayload!.Data!.Id}");
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

    private sealed class LanguagePayload
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
    }
}
