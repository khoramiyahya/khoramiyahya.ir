using System.Net;
using System.Net.Http.Json;

using PersonalBrand.IntegrationTests.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class StatusEndpointTests : IClassFixture<PersonalBrandWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StatusEndpointTests(PersonalBrandWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Status_Endpoint_Should_Return_Ok_Envelope()
    {
        var response = await _client.GetAsync("/api/v1/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
    }

    private sealed class StatusResponse
    {
        public bool Success { get; set; }

        public object? Data { get; set; }

        public string? Message { get; set; }

        public string? TraceId { get; set; }
    }
}
