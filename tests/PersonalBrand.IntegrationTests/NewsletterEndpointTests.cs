using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class NewsletterEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public NewsletterEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Subscribe_List_Unsubscribe_And_Delete_Should_Succeed()
    {
        var email = $"news_{Guid.NewGuid():N}@example.com";

        var subscribeResponse = await _client.PostAsJsonAsync("/api/v1/newsletter/subscribe", new
        {
            email,
            name = "Newsletter Reader"
        });

        Assert.Equal(HttpStatusCode.Created, subscribeResponse.StatusCode);
        var subscribePayload = await subscribeResponse.Content.ReadFromJsonAsync<ApiEnvelope<SubscriberPayload>>(JsonOptions);
        Assert.NotNull(subscribePayload?.Data);
        Assert.True(subscribePayload.Data.IsActive);
        Assert.Equal(email.ToLowerInvariant(), subscribePayload.Data.Email);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = ownerEmail,
            password,
            confirmPassword = password,
            displayName = "Newsletter Owner"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/newsletter/subscribers");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var listResponse = await _client.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listPayload = await listResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<SubscriberPayload>>>(JsonOptions);
        Assert.Contains(listPayload!.Data!, item => item.Id == subscribePayload.Data.Id);

        var unsubscribeResponse = await _client.PostAsJsonAsync("/api/v1/newsletter/unsubscribe", new { email });
        Assert.Equal(HttpStatusCode.NoContent, unsubscribeResponse.StatusCode);

        using var activeListRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/newsletter/subscribers?activeOnly=true");
        activeListRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var activeListResponse = await _client.SendAsync(activeListRequest);
        var activeList = await activeListResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<SubscriberPayload>>>(JsonOptions);
        Assert.DoesNotContain(activeList!.Data!, item => item.Id == subscribePayload.Data.Id);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/newsletter/subscribers/{subscribePayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Duplicate_Active_Subscribe_Should_Conflict()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";

        var first = await _client.PostAsJsonAsync("/api/v1/newsletter/subscribe", new { email });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/newsletter/subscribe", new { email });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Subscribers_Require_Authentication()
    {
        var response = await _client.GetAsync("/api/v1/newsletter/subscribers");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private sealed class SubscriberPayload
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
