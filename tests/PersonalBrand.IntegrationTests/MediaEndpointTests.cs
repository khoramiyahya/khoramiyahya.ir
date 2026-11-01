using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class MediaEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 1x1 transparent PNG
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly HttpClient _client;

    public MediaEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_List_Update_And_Delete_Media_Should_Succeed()
    {
        var email = $"media_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Media Owner"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TinyPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "gallery.png");
        form.Add(new StringContent("Hero image"), "title");
        form.Add(new StringContent("Brand hero"), "altText");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/media/me")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);

        var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<ApiEnvelope<MediaPayload>>(JsonOptions);
        Assert.NotNull(uploadPayload?.Data);
        Assert.Equal("gallery.png", uploadPayload.Data.FileName);
        Assert.StartsWith("/uploads/", uploadPayload.Data.Url);

        var fileResponse = await _client.GetAsync(uploadPayload.Data.Url);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/media/me");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var listResponse = await _client.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listPayload = await listResponse.Content.ReadFromJsonAsync<ApiEnvelope<List<MediaPayload>>>(JsonOptions);
        Assert.Contains(listPayload!.Data!, item => item.Id == uploadPayload.Data.Id);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/media/me/{uploadPayload.Data.Id}")
        {
            Content = JsonContent.Create(new
            {
                title = "Updated title",
                altText = "Updated alt"
            })
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<ApiEnvelope<MediaPayload>>(JsonOptions);
        Assert.Equal("Updated title", updatePayload?.Data?.Title);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/media/me/{uploadPayload.Data.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Upload_Invalid_Content_Type_Should_Fail()
    {
        var email = $"media_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Media Owner"
        });
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("not-an-image"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "notes.txt");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/media/me")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.BadRequest, uploadResponse.StatusCode);
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

    private sealed class MediaPayload
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? Title { get; set; }
    }
}
