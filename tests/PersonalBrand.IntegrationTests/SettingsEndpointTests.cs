using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class SettingsEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public SettingsEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Public_And_Update_Settings_Should_Succeed()
    {
        var publicResponse = await _client.GetAsync("/api/v1/settings/public");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicPayload = await publicResponse.Content.ReadFromJsonAsync<ApiEnvelope<SettingsPayload>>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(publicPayload?.Data?.SiteName));

        var email = $"settings_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Settings Owner"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/settings/me")
        {
            Content = JsonContent.Create(new
            {
                siteName = "Yahya Khorami",
                tagline = "Personal brand platform",
                metaTitle = "Yahya Khorami | Software Engineer",
                metaDescription = "Backend and platform engineering.",
                metaKeywords = "dotnet,backend,brand",
                canonicalBaseUrl = "https://khoramiyahya.ir",
                ogImageUrl = "https://khoramiyahya.ir/og.png",
                faviconUrl = "https://khoramiyahya.ir/favicon.ico",
                primaryEmail = "hello@khoramiyahya.ir",
                primaryPhone = "+989121234567",
                address = "Tehran, Iran",
                googleAnalyticsId = "G-TEST123",
                isMaintenanceMode = false
            })
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<ApiEnvelope<SettingsPayload>>(JsonOptions);
        Assert.Equal("Yahya Khorami", updatePayload?.Data?.SiteName);
        Assert.Equal("https://khoramiyahya.ir", updatePayload?.Data?.CanonicalBaseUrl);

        var refreshedPublic = await _client.GetAsync("/api/v1/settings/public");
        var refreshedPayload = await refreshedPublic.Content.ReadFromJsonAsync<ApiEnvelope<SettingsPayload>>(JsonOptions);
        Assert.Equal("Yahya Khorami", refreshedPayload?.Data?.SiteName);
    }

    [Fact]
    public async Task Update_Settings_Requires_Authentication()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/settings/me", new
        {
            siteName = "Unauthorized Site"
        });

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

    private sealed class SettingsPayload
    {
        public string SiteName { get; set; } = string.Empty;

        public string? CanonicalBaseUrl { get; set; }
    }
}
