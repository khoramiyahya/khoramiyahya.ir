using Microsoft.Data.SqlClient;

namespace PersonalBrand.IntegrationTests.Infrastructure;

public sealed class PersonalBrandWebApplicationFactoryLifecycleTests
{
    [Fact]
    public async Task DisposeAsync_Should_Drop_Only_The_Owned_Database()
    {
        var firstFactory = new PersonalBrandWebApplicationFactory();
        var secondFactory = new PersonalBrandWebApplicationFactory();

        try
        {
            using var firstClient = firstFactory.CreateClient();
            using var secondClient = secondFactory.CreateClient();

            Assert.True(await DatabaseExistsAsync(firstFactory));
            Assert.True(await DatabaseExistsAsync(secondFactory));

            await firstFactory.DisposeAsync();

            Assert.False(await DatabaseExistsAsync(firstFactory));
            Assert.True(await DatabaseExistsAsync(secondFactory));

            using var response = await secondClient.GetAsync("/api/v1/status");
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            await firstFactory.DisposeAsync();
            await secondFactory.DisposeAsync();
        }

        Assert.False(await DatabaseExistsAsync(secondFactory));
    }

    [Fact]
    public async Task Dispose_Should_Drop_Database_And_Be_Idempotent()
    {
        var factory = new PersonalBrandWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.True(await DatabaseExistsAsync(factory));

        factory.Dispose();
        factory.Dispose();

        Assert.False(await DatabaseExistsAsync(factory));
    }

    [Fact]
    public async Task Concurrent_Dispose_Calls_Should_Share_One_Teardown()
    {
        var factory = new PersonalBrandWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.True(await DatabaseExistsAsync(factory));

        var synchronousDisposal = Task.Run(factory.Dispose);
        var asynchronousDisposal = factory.DisposeAsync().AsTask();
        await Task.WhenAll(synchronousDisposal, asynchronousDisposal);

        Assert.False(await DatabaseExistsAsync(factory));
        Assert.False(Directory.Exists(factory.FileStorageRootPath));
    }

    private static async Task<bool> DatabaseExistsAsync(PersonalBrandWebApplicationFactory factory)
    {
        await using var connection = new SqlConnection(factory.MasterConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CASE WHEN DB_ID(@databaseName) IS NULL THEN 0 ELSE 1 END;";
        command.Parameters.AddWithValue("@databaseName", factory.DatabaseName);

        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }
}
