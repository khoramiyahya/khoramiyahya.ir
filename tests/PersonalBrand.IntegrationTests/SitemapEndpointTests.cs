using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class SitemapEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public SitemapEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Sitemap_Json_And_Xml_Should_Include_Published_Content()
    {
        var email = $"sitemap_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var uniqueToken = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Sitemap Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var projectRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = $"Sitemap Project {uniqueToken}",
                summary = "Published for sitemap coverage.",
                slug = $"sitemap-project-{uniqueToken}",
                isPublished = true
            })
        };
        projectRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var projectResponse = await _client.SendAsync(projectRequest);
        Assert.Equal(HttpStatusCode.Created, projectResponse.StatusCode);

        var jsonResponse = await _client.GetAsync("/api/v1/sitemap");
        Assert.Equal(HttpStatusCode.OK, jsonResponse.StatusCode);
        var jsonPayload = await jsonResponse.Content.ReadFromJsonAsync<ApiEnvelope<SitemapPayload>>(JsonOptions);
        Assert.NotNull(jsonPayload?.Data);
        Assert.Contains(jsonPayload.Data.Entries, entry => entry.Loc.Contains($"sitemap-project-{uniqueToken}"));
        Assert.Contains(jsonPayload.Data.Entries, entry => entry.ContentType == "home");

        var xmlResponse = await _client.GetAsync("/api/v1/sitemap/xml");
        Assert.Equal(HttpStatusCode.OK, xmlResponse.StatusCode);
        Assert.Equal("application/xml", xmlResponse.Content.Headers.ContentType?.MediaType);
        var xml = await xmlResponse.Content.ReadAsStringAsync();
        Assert.Contains($"sitemap-project-{uniqueToken}", xml);
        Assert.Contains("<urlset", xml);
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

    private sealed class SitemapPayload
    {
        public int TotalCount { get; set; }

        public List<SitemapEntryPayload> Entries { get; set; } = [];
    }

    private sealed class SitemapEntryPayload
    {
        public string Loc { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
