using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class RobotsEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public RobotsEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Published_Rule_Should_Appear_In_Public_Robots_Text()
    {
        var email = $"robots_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var path = $"/private-{Guid.NewGuid():N}"[..20];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Robots Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/robots/me")
        {
            Content = JsonContent.Create(new
            {
                userAgent = "*",
                directive = "Disallow",
                path,
                displayOrder = 1,
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<RobotsPayload>>(JsonOptions);
        Assert.True(createPayload?.Data?.IsPublished);

        var publicResponse = await _client.GetAsync("/api/v1/robots/public");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        Assert.Equal("text/plain", publicResponse.Content.Headers.ContentType?.MediaType);
        var text = await publicResponse.Content.ReadAsStringAsync();
        Assert.Contains($"Disallow: {path}", text);
        Assert.Contains("Sitemap:", text);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/robots/me/{createPayload!.Data!.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
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

    private sealed class RobotsPayload
    {
        public Guid Id { get; set; }

        public bool IsPublished { get; set; }
    }
}
