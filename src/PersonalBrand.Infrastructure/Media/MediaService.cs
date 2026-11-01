using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Media;
using PersonalBrand.Application.Media.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Media;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Media;

internal sealed class MediaService : IMediaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IFileStorage _fileStorage;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IValidator<UpdateMediaAssetRequest> _updateValidator;

    public MediaService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IFileStorage fileStorage,
        IOptions<FileStorageOptions> fileStorageOptions,
        IValidator<UpdateMediaAssetRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _fileStorage = fileStorage;
        _fileStorageOptions = fileStorageOptions.Value;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<MediaAssetDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var assets = await _dbContext.MediaAssets
            .AsNoTracking()
            .Where(asset => asset.UserId == userId)
            .OrderByDescending(asset => asset.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return assets.Select(Map).ToArray();
    }

    public async Task<MediaAssetDto> GetMineByIdAsync(
        Guid mediaId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var asset = await GetOwnedAsync(userId, mediaId, tracking: false, cancellationToken);
        return Map(asset);
    }

    public async Task<MediaAssetDto> UploadMineAsync(
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        string? title,
        string? altText,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        ValidateMedia(fileName, contentType, contentLength);

        var userId = RequireCurrentUserId();

        await using var limitedStream = await CopyToLimitedMemoryStreamAsync(
            content,
            _fileStorageOptions.MediaMaxBytes,
            cancellationToken);

        var storedPath = await _fileStorage.SaveAsync(
            limitedStream,
            fileName,
            contentType,
            cancellationToken);

        MediaAsset asset;
        try
        {
            asset = MediaAsset.Create(
                userId,
                Path.GetFileName(fileName),
                storedPath,
                contentType,
                limitedStream.Length,
                _dateTimeProvider.UtcNow,
                title,
                altText);
        }
        catch (DomainException exception)
        {
            await _fileStorage.DeleteAsync(storedPath, cancellationToken);
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Media"] = [exception.Message]
            });
        }

        _dbContext.MediaAssets.Add(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "media.upload",
            entityName: nameof(MediaAsset),
            entityId: asset.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(asset);
    }

    public async Task<MediaAssetDto> UpdateMineAsync(
        Guid mediaId,
        UpdateMediaAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var asset = await GetOwnedAsync(userId, mediaId, tracking: true, cancellationToken);

        try
        {
            asset.UpdateDetails(request.Title, request.AltText, _dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Media"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "media.update",
            entityName: nameof(MediaAsset),
            entityId: asset.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(asset);
    }

    public async Task DeleteMineAsync(Guid mediaId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var asset = await GetOwnedAsync(userId, mediaId, tracking: true, cancellationToken);
        var storedPath = asset.StoredPath;

        _dbContext.MediaAssets.Remove(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _fileStorage.DeleteAsync(storedPath, cancellationToken);

        await _auditLogger.LogAsync(
            action: "media.delete",
            entityName: nameof(MediaAsset),
            entityId: mediaId.ToString(),
            cancellationToken: cancellationToken);
    }

    private void ValidateMedia(string fileName, string contentType, long? contentLength)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Media file name is required."]
            });
        }

        if (contentLength is > 0 && contentLength > _fileStorageOptions.MediaMaxBytes)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = [$"Media must be at most {_fileStorageOptions.MediaMaxBytes / 1024 / 1024} MB."]
            });
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension)
            || !_fileStorageOptions.AllowedMediaExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Media extension is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedMediaExtensions)}"
                ]
            });
        }

        if (string.IsNullOrWhiteSpace(contentType)
            || !_fileStorageOptions.AllowedMediaContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Media content type is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedMediaContentTypes)}"
                ]
            });
        }
    }

    private static async Task<MemoryStream> CopyToLimitedMemoryStreamAsync(
        Stream content,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        var buffer = new byte[81920];
        long totalRead = 0;

        while (true)
        {
            var read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
            if (totalRead > maxBytes)
            {
                await memoryStream.DisposeAsync();
                throw new AppValidationException(new Dictionary<string, string[]>
                {
                    ["file"] = [$"Media must be at most {maxBytes / 1024 / 1024} MB."]
                });
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (totalRead == 0)
        {
            await memoryStream.DisposeAsync();
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Media file is empty."]
            });
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<MediaAsset> GetOwnedAsync(
        string userId,
        Guid mediaId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = tracking
            ? _dbContext.MediaAssets.AsQueryable()
            : _dbContext.MediaAssets.AsNoTracking();

        var asset = await query.SingleOrDefaultAsync(
            item => item.Id == mediaId && item.UserId == userId,
            cancellationToken);

        if (asset is null)
        {
            throw new NotFoundException("MediaAsset", mediaId);
        }

        return asset;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private MediaAssetDto Map(MediaAsset asset)
    {
        var requestPath = _fileStorageOptions.RequestPath.TrimEnd('/');
        return new MediaAssetDto
        {
            Id = asset.Id,
            FileName = asset.FileName,
            StoredPath = asset.StoredPath,
            Url = $"{requestPath}/{asset.StoredPath.TrimStart('/')}",
            ContentType = asset.ContentType,
            SizeBytes = asset.SizeBytes,
            Title = asset.Title,
            AltText = asset.AltText,
            CreatedAtUtc = asset.CreatedAtUtc,
            UpdatedAtUtc = asset.UpdatedAtUtc
        };
    }
}
