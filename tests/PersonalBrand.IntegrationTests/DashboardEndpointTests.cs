using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class DashboardEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public DashboardEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Summary_Should_Reflect_Created_Content()
    {
        var email = $"dashboard_{Guid.NewGuid():N}@example.com";
        var password = "StrongPass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            confirmPassword = password,
            displayName = "Dashboard Owner"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authPayload = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthPayload>>(JsonOptions);
        var accessToken = authPayload!.Data!.AccessToken;

        using var createProject = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Dashboard Project",
                slug = $"dash-project-{Guid.NewGuid():N}"[..24],
                isPublished = true
            })
        };
        createProject.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(createProject)).StatusCode);

        using var createBlog = new HttpRequestMessage(HttpMethod.Post, "/api/v1/blog/me")
        {
            Content = JsonContent.Create(new
            {
                title = "Dashboard Post",
                slug = $"dash-post-{Guid.NewGuid():N}"[..24],
                body = "Published body content for dashboard test.",
                isPublished = true
            })
        };
        createBlog.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(createBlog)).StatusCode);

        var contactResponse = await _client.PostAsJsonAsync("/api/v1/contact", new
        {
            name = "Visitor",
            email = $"visitor_{Guid.NewGuid():N}@example.com",
            subject = "Hello",
            body = "Please review my request."
        });
        Assert.Equal(HttpStatusCode.Created, contactResponse.StatusCode);

        var subscribeResponse = await _client.PostAsJsonAsync("/api/v1/newsletter/subscribe", new
        {
            email = $"reader_{Guid.NewGuid():N}@example.com"
        });
        Assert.Equal(HttpStatusCode.Created, subscribeResponse.StatusCode);

        using var summaryRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard/summary");
        summaryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var summaryResponse = await _client.SendAsync(summaryRequest);
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);

        var summary = await summaryResponse.Content.ReadFromJsonAsync<ApiEnvelope<DashboardPayload>>(JsonOptions);
        Assert.NotNull(summary?.Data);
        Assert.Equal(1, summary.Data.ProjectsTotal);
        Assert.Equal(1, summary.Data.ProjectsPublished);
        Assert.Equal(1, summary.Data.BlogPostsTotal);
        Assert.Equal(1, summary.Data.BlogPostsPublished);
        Assert.True(summary.Data.ContactMessagesTotal >= 1);
        Assert.True(summary.Data.ContactMessagesUnread >= 1);
        Assert.True(summary.Data.NewsletterSubscribersActive >= 1);
    }

    [Fact]
    public async Task Summary_Requires_Authentication()
    {
        var response = await _client.GetAsync("/api/v1/dashboard/summary");
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

    private sealed class DashboardPayload
    {
        public int ProjectsTotal { get; set; }

        public int ProjectsPublished { get; set; }

        public int BlogPostsTotal { get; set; }

        public int BlogPostsPublished { get; set; }

        public int ContactMessagesTotal { get; set; }

        public int ContactMessagesUnread { get; set; }

        public int NewsletterSubscribersActive { get; set; }
    }
}
