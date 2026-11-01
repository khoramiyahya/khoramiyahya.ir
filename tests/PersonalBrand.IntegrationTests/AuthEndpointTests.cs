using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class AuthEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public AuthEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_And_Me_Should_Succeed()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Test User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        Assert.NotNull(registerPayload);
        Assert.True(registerPayload.Success);
        Assert.False(string.IsNullOrWhiteSpace(registerPayload.Data?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(registerPayload.Data?.RefreshToken));
        Assert.Contains("Viewer", registerPayload.Data!.User.Roles);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        Assert.NotNull(loginPayload?.Data?.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload.Data.RefreshToken));

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload!.Data!.AccessToken);

        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<ApiEnvelope<UserPayload>>(JsonOptions);
        Assert.NotNull(mePayload);
        Assert.True(mePayload.Success);
        Assert.Equal(email, mePayload.Data?.Email);
        Assert.Equal("Test User", mePayload.Data?.DisplayName);
    }

    [Fact]
    public async Task Refresh_Should_Rotate_Tokens_And_Reject_Reuse()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Refresh User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var originalRefreshToken = registerPayload!.Data!.RefreshToken;

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = originalRefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshPayload = await refreshResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        Assert.NotNull(refreshPayload?.Data);
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.Data.RefreshToken));
        Assert.NotEqual(originalRefreshToken, refreshPayload.Data.RefreshToken);

        var reuseResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = originalRefreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Revoke_Should_Invalidate_Refresh_Token()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Revoke User"
        });

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var refreshToken = registerPayload!.Data!.RefreshToken;

        var revokeResponse = await _client.PostAsJsonAsync("/api/v1/auth/revoke", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Login_With_Invalid_Password_Should_Return_Unauthorized()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Test User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "WrongPass1!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }

        public string? Message { get; set; }
    }

    private sealed class AuthPayload
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public UserPayload User { get; set; } = new();
    }

    private sealed class UserPayload
    {
        public string Email { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public List<string> Roles { get; set; } = [];
    }
}
