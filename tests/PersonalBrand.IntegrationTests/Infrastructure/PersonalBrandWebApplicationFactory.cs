using System.Data;
using System.Runtime.ExceptionServices;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalBrand.IntegrationTests.Infrastructure;

public sealed class PersonalBrandWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string DatabaseNamePrefix = "PersonalBrandPlatform_Test_";

    private readonly Lock _disposeLock = new();
    private readonly string _databaseName = $"{DatabaseNamePrefix}{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    private readonly string _fileStorageRootPath;
    private readonly Action<IServiceCollection>? _configureServices;

    private Task? _disposeTask;

    public PersonalBrandWebApplicationFactory()
        : this(null)
    {
    }

    internal PersonalBrandWebApplicationFactory(Action<IServiceCollection>? configureServices)
    {
        _configureServices = configureServices;
        _fileStorageRootPath = Path.Combine(
            Path.GetTempPath(),
            "PersonalBrandPlatformTests",
            _databaseName);

        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\mssqllocaldb",
            InitialCatalog = _databaseName,
            IntegratedSecurity = true,
            MultipleActiveResultSets = true,
            TrustServerCertificate = true
        };

        _connectionString = connectionStringBuilder.ConnectionString;
        connectionStringBuilder.InitialCatalog = "master";
        _masterConnectionString = connectionStringBuilder.ConnectionString;
    }

    internal string DatabaseName => _databaseName;

    internal string MasterConnectionString => _masterConnectionString;

    internal string FileStorageRootPath => _fileStorageRootPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _connectionString);
        builder.UseSetting("Jwt:Issuer", "PersonalBrandPlatform.Test");
        builder.UseSetting("Jwt:Audience", "PersonalBrandPlatform.Clients.Test");
        builder.UseSetting("Jwt:SigningKey", "TEST_ONLY_SIGNING_KEY_MUST_BE_AT_LEAST_32_CHARS");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "30");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:5173");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("FileStorage:RootPath", _fileStorageRootPath);
        builder.UseSetting("BootstrapAdmin:Email", string.Empty);
        builder.UseSetting("BootstrapAdmin:Password", string.Empty);

        if (_configureServices is not null)
        {
            builder.ConfigureServices(_configureServices);
        }
    }

    public override ValueTask DisposeAsync() => new(GetDisposeTask());

    private Task GetDisposeTask()
    {
        lock (_disposeLock)
        {
            return _disposeTask ??= DisposeAndCleanupAsync();
        }
    }

    private async Task DisposeAndCleanupAsync()
    {
        Exception? disposalException = null;
        Exception? cleanupException = null;

        try
        {
            await base.DisposeAsync();
        }
        catch (Exception exception)
        {
            disposalException = exception;
        }

        try
        {
            await CleanupTestResourcesAsync();
        }
        catch (Exception exception)
        {
            cleanupException = exception;
        }

        ThrowDisposalExceptions(disposalException, cleanupException);
    }

    private async Task CleanupTestResourcesAsync()
    {
        var exceptions = new List<Exception>();

        try
        {
            await DropTestDatabaseAsync();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        try
        {
            DeleteTestFileStorage();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more integration-test resources could not be removed.", exceptions);
        }
    }

    private async Task DropTestDatabaseAsync()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
        if (!connectionStringBuilder.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
            || !_databaseName.StartsWith(DatabaseNamePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to drop a database outside the integration-test LocalDB boundary.");
        }

        using (var pooledConnection = new SqlConnection(_connectionString))
        {
            SqlConnection.ClearPool(pooledConnection);
        }

        const string dropDatabaseSql = """
            IF DB_ID(@databaseName) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) =
                    N'ALTER DATABASE ' + QUOTENAME(@databaseName)
                    + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '
                    + QUOTENAME(@databaseName) + N';';
                EXEC sys.sp_executesql @sql;
            END
            """;

        const int maximumAttempts = 3;
        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                await using var masterConnection = new SqlConnection(_masterConnectionString);
                await masterConnection.OpenAsync();

                await using var command = masterConnection.CreateCommand();
                command.CommandText = dropDatabaseSql;
                command.Parameters.Add(new SqlParameter("@databaseName", SqlDbType.NVarChar, 128)
                {
                    Value = _databaseName
                });

                await command.ExecuteNonQueryAsync();
                return;
            }
            catch (SqlException) when (attempt < maximumAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt));
            }
        }
    }

    private void DeleteTestFileStorage()
    {
        var testStorageParent = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "PersonalBrandPlatformTests"));
        var storageRoot = Path.GetFullPath(_fileStorageRootPath);
        var parentPrefix = testStorageParent.EndsWith(Path.DirectorySeparatorChar)
            ? testStorageParent
            : testStorageParent + Path.DirectorySeparatorChar;

        if (!storageRoot.StartsWith(parentPrefix, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetFileName(storageRoot), _databaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to remove file storage outside the integration-test boundary.");
        }

        if (Directory.Exists(storageRoot))
        {
            Directory.Delete(storageRoot, recursive: true);
        }

        var transactionRoot = Path.Combine(testStorageParent, $".{_databaseName}.transactions");
        if (Directory.Exists(transactionRoot))
        {
            Directory.Delete(transactionRoot, recursive: true);
        }
    }

    private static void ThrowDisposalExceptions(Exception? disposalException, Exception? cleanupException)
    {
        if (disposalException is not null && cleanupException is not null)
        {
            throw new AggregateException(disposalException, cleanupException);
        }

        var exception = disposalException ?? cleanupException;
        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
