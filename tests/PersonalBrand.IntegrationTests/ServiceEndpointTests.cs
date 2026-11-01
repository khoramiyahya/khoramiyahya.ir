using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class ServiceEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public ServiceEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Publish_List_And_Delete_Service_Should_Succeed()
    {
        var email = $"service_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Service Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        var slug = $"consulting-{Guid.NewGuid():N}"[..24];

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/services/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Architecture Consulting",
                slug,
                summary = "Backend and platform consulting.",
                description = "Clean Architecture and .NET platform advisory.",
                priceLabel = "From $100/hr",
                iconName = "architecture",
                displayOrder = 1,
                isFeatured = true,
                isPublished = true
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<ServicePayload>>(JsonOptions);
        Assert.NotNull(createPayload?.Data);
        Assert.True(createPayload.Data.IsPublished);
        Assert.Equal("Architecture Consulting", createPayload.Data.Title);

        var publicListResponse = await _client.GetAsync("/api/v1/services/public");
        Assert.Equal(HttpStatusCode.OK, publicListResponse.StatusCode);
        var publicList = await publicListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<ServicePayload>>>(JsonOptions);
        Assert.Contains(publicList!.Data!, item => item.Slug == slug);

        var publicItemResponse = await _client.GetAsync($"/api/v1/services/public/{slug}");
        Assert.Equal(HttpStatusCode.OK, publicItemResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/services/me/{createPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/v1/services/public/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Unpublished_Service_Should_Not_Appear_In_Public_List()
    {
        var email = $"service_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft Service Owner"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;
        var slug = $"draft-svc-{Guid.NewGuid():N}"[..24];

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/services/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Draft Service",
                slug,
                isPublished = false
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var publicResponse = await _client.GetAsync($"/api/v1/services/public/{slug}");
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

    private sealed class ServicePayload
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
    }
}
