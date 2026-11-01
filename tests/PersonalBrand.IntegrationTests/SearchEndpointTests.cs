using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class SearchEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public SearchEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_Should_Return_Published_Project_And_Ignore_Draft()
    {
        var email = $"search_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var uniqueToken = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Search Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var publishedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = $"Published Searchable {uniqueToken}",
                summary = "A published project for search coverage.",
                description = "Detailed description.",
                isPublished = true
            })
        };
        publishedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var publishedResponse = await _client.SendAsync(publishedRequest);
        Assert.Equal(HttpStatusCode.Created, publishedResponse.StatusCode);

        using var draftRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = $"Draft Searchable {uniqueToken}",
                summary = "A draft project that should not appear.",
                isPublished = false
            })
        };
        draftRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var draftResponse = await _client.SendAsync(draftRequest);
        Assert.Equal(HttpStatusCode.Created, draftResponse.StatusCode);

        var searchResponse = await _client.GetAsync($"/api/v1/search?q={uniqueToken}&contentType=project");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var searchPayload = await searchResponse.Content.ReadFromJsonAsync<ApiEnvelope<SearchPayload>>(JsonOptions);
        Assert.NotNull(searchPayload?.Data);
        Assert.Contains(searchPayload.Data.Items, item => item.Title.Contains($"Published Searchable {uniqueToken}"));
        Assert.DoesNotContain(searchPayload.Data.Items, item => item.Title.Contains($"Draft Searchable {uniqueToken}"));
    }

    [Fact]
    public async Task Search_With_Short_Query_Should_Fail_Validation()
    {
        var response = await _client.GetAsync("/api/v1/search?q=a");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private sealed class SearchPayload
    {
        public string Query { get; set; } = string.Empty;

        public int TotalCount { get; set; }

        public List<SearchItemPayload> Items { get; set; } = [];
    }

    private sealed class SearchItemPayload
    {
        public Guid Id { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
    }
}
