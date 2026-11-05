using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure.Options;

namespace PersonalBrand.Infrastructure.Storage;

/// <summary>
/// Stores files on the local file system. Suitable for IIS shared hosting when a writable folder is available.
/// </summary>
internal sealed class LocalFileStorage : IFileStorage, IFileStorageTransaction
{
    private readonly FileStorageOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LocalFileStorage> _logger;

    private readonly List<PendingSave> _pendingSaves = [];
    private readonly List<string> _pendingDeletes = [];
    private readonly List<string> _preparedSaves = [];

    private bool _transactionActive;
    private string? _transactionFolder;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment environment,
        ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public void Begin()
    {
        if (_transactionActive)
        {
            throw new InvalidOperationException("A file-storage transaction is already active in this scope.");
        }

        _transactionActive = true;
        _transactionFolder = Path.Combine(
            ResolveTransactionRootPath(),
            Guid.NewGuid().ToString("N"));
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
        var storedFileName = $"{Guid.NewGuid():N}_{safeFileName}";
        var relativePath = Path.Combine(relativeFolder, storedFileName).Replace('\\', '/');
        var absolutePath = ResolveAbsolutePath(relativePath);

        if (_transactionActive)
        {
            var transactionFolder = GetTransactionFolder();
            Directory.CreateDirectory(transactionFolder);
            var stagedPath = Path.Combine(transactionFolder, $"{Guid.NewGuid():N}.staged");

            try
            {
                await using var stagedStream = File.Create(stagedPath);
                await content.CopyToAsync(stagedStream, cancellationToken);
            }
            catch
            {
                DeleteIfExists(stagedPath);
                throw;
            }

            _pendingSaves.Add(new PendingSave(relativePath, stagedPath, absolutePath));
            return relativePath;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Stored file {RelativePath} with content type {ContentType}", relativePath, contentType);
        return relativePath;
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var absolutePath = ResolveAbsolutePath(relativePath);

        if (_transactionActive)
        {
            var pendingSave = _pendingSaves.Find(save =>
                string.Equals(save.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));
            if (pendingSave is not null)
            {
                DeleteIfExists(pendingSave.StagedPath);
                _pendingSaves.Remove(pendingSave);
                return Task.CompletedTask;
            }

            if (!_pendingDeletes.Contains(absolutePath, StringComparer.OrdinalIgnoreCase))
            {
                _pendingDeletes.Add(absolutePath);
            }

            return Task.CompletedTask;
        }

        DeleteIfExists(absolutePath);
        return Task.CompletedTask;
    }

    public Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        EnsureTransactionActive();

        foreach (var pendingSave in _pendingSaves)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(pendingSave.AbsolutePath)!);
            File.Move(pendingSave.StagedPath, pendingSave.AbsolutePath);
            _preparedSaves.Add(pendingSave.AbsolutePath);
        }

        return Task.CompletedTask;
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        EnsureTransactionActive();

        try
        {
            foreach (var absolutePath in _pendingDeletes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    DeleteIfExists(absolutePath);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    _logger.LogWarning(
                        exception,
                        "Could not remove file {FilePath} after the database transaction committed.",
                        absolutePath);
                }
            }

            try
            {
                TryDeleteTransactionFolder();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(exception, "Could not remove the committed file transaction folder.");
            }
        }
        finally
        {
            ResetTransaction();
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (!_transactionActive)
        {
            return Task.CompletedTask;
        }

        var exceptions = new List<Exception>();

        foreach (var absolutePath in _preparedSaves.AsEnumerable().Reverse())
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryFileOperation(() => DeleteIfExists(absolutePath), exceptions);
        }

        foreach (var pendingSave in _pendingSaves)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryFileOperation(() => DeleteIfExists(pendingSave.StagedPath), exceptions);
        }

        TryFileOperation(TryDeleteTransactionFolder, exceptions);
        ResetTransaction();

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more staged file operations could not be rolled back.", exceptions);
        }

        return Task.CompletedTask;
    }

    public Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (!_transactionActive)
        {
            return Task.CompletedTask;
        }

        foreach (var pendingSave in _pendingSaves)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteIfExists(pendingSave.StagedPath);
        }

        TryDeleteTransactionFolder();
        ResetTransaction();
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

    private string ResolveAbsolutePath(string relativePath)
    {
        var root = Path.GetFullPath(ResolveRootPath());
        var absolutePath = Path.GetFullPath(Path.Combine(
            root,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!absolutePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The file path is outside the configured storage root.");
        }

        return absolutePath;
    }

    private string ResolveTransactionRootPath()
    {
        var storageRoot = Path.GetFullPath(ResolveRootPath());
        var parent = Directory.GetParent(storageRoot)?.FullName
            ?? throw new InvalidOperationException("The configured storage root must have a parent directory.");
        var storageName = Path.GetFileName(storageRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar));

        return Path.Combine(parent, $".{storageName}.transactions");
    }

    private string GetTransactionFolder() =>
        _transactionFolder
        ?? throw new InvalidOperationException("A file-storage transaction has not been started.");

    private void EnsureTransactionActive()
    {
        if (!_transactionActive)
        {
            throw new InvalidOperationException("A file-storage transaction has not been started.");
        }
    }

    private void TryDeleteTransactionFolder()
    {
        if (_transactionFolder is not null && Directory.Exists(_transactionFolder))
        {
            Directory.Delete(_transactionFolder, recursive: true);

            var transactionRoot = Path.GetDirectoryName(_transactionFolder);
            if (transactionRoot is not null
                && Directory.Exists(transactionRoot)
                && !Directory.EnumerateFileSystemEntries(transactionRoot).Any())
            {
                try
                {
                    Directory.Delete(transactionRoot);
                }
                catch (IOException)
                {
                    // A concurrent request may have created another transaction folder.
                }
            }
        }
    }

    private void ResetTransaction()
    {
        _pendingSaves.Clear();
        _pendingDeletes.Clear();
        _preparedSaves.Clear();
        _transactionFolder = null;
        _transactionActive = false;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void TryFileOperation(Action operation, ICollection<Exception> exceptions)
    {
        try
        {
            operation();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }
    }

    private sealed record PendingSave(string RelativePath, string StagedPath, string AbsolutePath);
}
