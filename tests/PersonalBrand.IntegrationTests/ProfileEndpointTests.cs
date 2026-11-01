using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class ProfileEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public ProfileEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upsert_Publish_And_Get_Public_Profile_Should_Succeed()
    {
        var email = $"profile_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Yahya KHorami"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var upsertRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/profiles/me");
        upsertRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        upsertRequest.Content = JsonContent.Create(new
        {
            displayName = "Yahya KHorami",
            slug = $"yahya-{Guid.NewGuid():N}"[..18],
            headline = "Software Architect",
            bio = "Building personal brand platforms.",
            location = "Iran",
            websiteUrl = "https://khoramiyahya.ir",
            isPublished = true
        });

        var upsertResponse = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);

        var upsertPayload = await upsertResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProfilePayload>>(JsonOptions);
        Assert.NotNull(upsertPayload?.Data);
        Assert.True(upsertPayload.Data.IsPublished);
        Assert.Equal("Software Architect", upsertPayload.Data.Headline);

        var slug = upsertPayload.Data.Slug;
        var publicResponse = await _client.GetAsync($"/api/v1/profiles/public/{slug}");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);

        var publicPayload = await publicResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProfilePayload>>(JsonOptions);
        Assert.Equal("Yahya KHorami", publicPayload?.Data?.DisplayName);
        Assert.Equal("https://khoramiyahya.ir", publicPayload?.Data?.WebsiteUrl);
    }

    [Fact]
    public async Task Unpublished_Profile_Should_Not_Be_Public()
    {
        var email = $"profile_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Draft User"
        });

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/profiles/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<ApiEnvelope<ProfilePayload>>(JsonOptions);
        Assert.False(mePayload!.Data!.IsPublished);

        var publicResponse = await _client.GetAsync($"/api/v1/profiles/public/{mePayload.Data.Slug}");
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

    private sealed class ProfilePayload
    {
        public string Slug { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Headline { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsPublished { get; set; }
    }
}
