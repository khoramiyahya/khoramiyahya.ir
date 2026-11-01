using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class AvatarUploadTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 1x1 transparent PNG
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly HttpClient _client;

    public AvatarUploadTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_And_Remove_Avatar_Should_Succeed()
    {
        var email = $"avatar_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Avatar User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(TinyPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "avatar.png");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/profiles/me/avatar")
        {
            Content = form
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadPayload = await uploadResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProfilePayload>>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload?.Data?.AvatarPath));
        Assert.False(string.IsNullOrWhiteSpace(uploadPayload?.Data?.AvatarUrl));
        Assert.StartsWith("/uploads/", uploadPayload!.Data!.AvatarUrl);

        var fileResponse = await _client.GetAsync(uploadPayload.Data.AvatarUrl);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/profiles/me/avatar");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProfilePayload>>(JsonOptions);
        Assert.Null(deletePayload?.Data?.AvatarPath);
        Assert.Null(deletePayload?.Data?.AvatarUrl);
    }

    [Fact]
    public async Task Upload_Invalid_Content_Type_Should_Fail()
    {
        var email = $"avatar_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Avatar User"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("not-an-image"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "notes.txt");

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/profiles/me/avatar")
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

    private sealed class ProfilePayload
    {
        public string? AvatarPath { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
