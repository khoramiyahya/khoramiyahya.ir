using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class SchemaEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SchemaEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Schema_Should_Include_Published_Profile_And_Social_Link()
    {
        var email = $"schema_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var token = Guid.NewGuid().ToString("N")[..8];
        var displayName = $"Schema Owner {token}";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>();
        var accessToken = authPayload!.Data!.AccessToken;

        using var profileRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/profiles/me")
        {
            Content = JsonContent.Create(new
            {
                displayName,
                headline = "Platform engineer",
                bio = "Builds personal brand platforms.",
                slug = $"schema-owner-{token}",
                websiteUrl = "https://example.com",
                isPublished = true
            })
        };
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var profileResponse = await _client.SendAsync(profileRequest);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        using var skillRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/skills/me")
        {
            Content = JsonContent.Create(new
            {
                name = $"Architecture-{token}",
                proficiencyPercent = 90,
                isPublished = true
            })
        };
        skillRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var skillResponse = await _client.SendAsync(skillRequest);
        Assert.Equal(HttpStatusCode.Created, skillResponse.StatusCode);

        using var socialRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sociallinks/me")
        {
            Content = JsonContent.Create(new
            {
                platform = "GitHub",
                label = "GitHub",
                url = $"https://github.com/schema-{token}",
                isPublished = true
            })
        };
        socialRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var socialResponse = await _client.SendAsync(socialRequest);
        Assert.Equal(HttpStatusCode.Created, socialResponse.StatusCode);

        var schemaResponse = await _client.GetAsync("/api/v1/schema");
        Assert.Equal(HttpStatusCode.OK, schemaResponse.StatusCode);
        Assert.Equal("application/ld+json", schemaResponse.Content.Headers.ContentType?.MediaType);

        var json = await schemaResponse.Content.ReadAsStringAsync();
        Assert.Contains("https://schema.org", json);
        Assert.Contains(displayName, json);
        Assert.Contains("Platform engineer", json);
        Assert.Contains($"https://github.com/schema-{token}", json);
        Assert.Contains($"Architecture-{token}", json);
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
}
