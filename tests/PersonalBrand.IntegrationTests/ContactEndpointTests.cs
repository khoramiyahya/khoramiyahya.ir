using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class ContactEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public ContactEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Submit_List_MarkRead_And_Delete_Should_Succeed()
    {
        var submitResponse = await _client.PostAsJsonAsync("/api/v1/contact", new
        {
            name = "Visitor Name",
            email = $"visitor_{Guid.NewGuid():N}@example.com",
            phone = "+989121234567",
            subject = "Project inquiry",
            body = "I would like to discuss a collaboration opportunity."
        });

        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var submitPayload = await submitResponse.Content.ReadFromJsonAsync<ApiEnvelope<ContactPayload>>(JsonOptions);
        Assert.NotNull(submitPayload?.Data);
        Assert.False(submitPayload.Data.IsRead);
        Assert.Equal("Project inquiry", submitPayload.Data.Subject);

        var email = $"owner_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Inbox Owner"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var inboxRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/contact/inbox");
        inboxRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var inboxResponse = await _client.SendAsync(inboxRequest);
        Assert.Equal(HttpStatusCode.OK, inboxResponse.StatusCode);
        var inboxPayload = await inboxResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<ContactPayload>>>(JsonOptions);
        Assert.Contains(inboxPayload!.Data!, item => item.Id == submitPayload.Data.Id);

        using var readRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/contact/inbox/{submitPayload.Data.Id}/read");
        readRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var readResponse = await _client.SendAsync(readRequest);
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        var readPayload = await readResponse.Content.ReadFromJsonAsync<ApiEnvelope<ContactPayload>>(JsonOptions);
        Assert.True(readPayload?.Data?.IsRead);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/contact/inbox/{submitPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var missingRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/contact/inbox/{submitPayload.Data.Id}");
        missingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var missingResponse = await _client.SendAsync(missingRequest);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Invalid_Contact_Submission_Should_Fail()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/contact", new
        {
            name = "",
            email = "not-an-email",
            subject = "",
            body = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Inbox_Requires_Authentication()
    {
        var response = await _client.GetAsync("/api/v1/contact/inbox");
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

    private sealed class ContactPayload
    {
        public Guid Id { get; set; }

        public string Subject { get; set; } = string.Empty;

        public bool IsRead { get; set; }
    }
}
