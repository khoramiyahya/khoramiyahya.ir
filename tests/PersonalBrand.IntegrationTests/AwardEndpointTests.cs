using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class AwardEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public AwardEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Publish_List_And_Delete_Award_Should_Succeed()
    {
        var email = $"award_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Award Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/awards/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Design Excellence",
                issuer = "Regional Design Council",
                description = "Awarded for product design leadership.",
                url = "https://example.com/awards/design",
                awardedOn = "2024-06-01",
                displayOrder = 1,
                isFeatured = true,
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<AwardPayload>>(JsonOptions);
        Assert.NotNull(createPayload?.Data);
        Assert.True(createPayload.Data.IsPublished);
        Assert.Equal("Design Excellence", createPayload.Data.Title);

        var publicListResponse = await _client.GetAsync("/api/v1/awards/public");
        Assert.Equal(HttpStatusCode.OK, publicListResponse.StatusCode);
        var publicList = await publicListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<AwardPayload>>>(JsonOptions);
        Assert.Contains(publicList!.Data!, item => item.Id == createPayload.Data.Id);

        var publicItemResponse = await _client.GetAsync($"/api/v1/awards/public/{createPayload.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, publicItemResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/awards/me/{createPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/v1/awards/public/{createPayload.Data.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Unpublished_Award_Should_Not_Appear_In_Public_List()
    {
        var email = $"award_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft Award Owner"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/awards/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Draft Honor",
                issuer = "Local Guild",
                awardedOn = "2023-01-15",
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<AwardPayload>>(JsonOptions);

        var publicResponse = await _client.GetAsync($"/api/v1/awards/public/{createPayload!.Data!.Id}");
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

    private sealed class AwardPayload
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
    }
}
