using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Options;

namespace PersonalBrand.Infrastructure.Storage;

/// <summary>
/// Stores files on the local file system. Suitable for IIS shared hosting when a writable folder is available.
/// </summary>
internal sealed class LocalFileStorage : IFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment environment,
        ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var safeFileName = Path.GetFileName(fileName);
        var relativeFolder = DateTime.UtcNow.ToString("yyyy/MM");
        var root = ResolveRootPath();
        var absoluteFolder = Path.Combine(root, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var storedFileName = $"{Guid.NewGuid():N}_{safeFileName}";
        var absolutePath = Path.Combine(absoluteFolder, storedFileName);

        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        var relativePath = Path.Combine(relativeFolder, storedFileName).Replace('\\', '/');
        _logger.LogInformation("Stored file {RelativePath} with content type {ContentType}", relativePath, contentType);
        return relativePath;
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var absolutePath = Path.Combine(ResolveRootPath(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string ResolveRootPath()
    {
        if (Path.IsPathRooted(_options.RootPath))
        {
            return _options.RootPath;
        }

        return Path.Combine(_environment.ContentRootPath, _options.RootPath);
    }
}
