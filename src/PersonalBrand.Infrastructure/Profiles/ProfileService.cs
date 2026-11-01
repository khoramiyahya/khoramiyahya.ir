using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Profiles;
using PersonalBrand.Application.Profiles.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Profiles;

internal sealed class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IFileStorage _fileStorage;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IValidator<UpdateProfileRequest> _updateValidator;

    public ProfileService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IFileStorage fileStorage,
        IOptions<FileStorageOptions> fileStorageOptions,
        IValidator<UpdateProfileRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _fileStorage = fileStorage;
        _fileStorageOptions = fileStorageOptions.Value;
        _updateValidator = updateValidator;
    }

    public async Task<ProfileDto> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException("Published profile was not found.");
        }

        string normalizedSlug;
        try
        {
            normalizedSlug = ProfileSlug.Normalize(slug);
        }
        catch (DomainException)
        {
            throw new NotFoundException("Published profile was not found.");
        }

        var profile = await _dbContext.BrandProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug && item.IsPublished,
                cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("BrandProfile", normalizedSlug);
        }

        return Map(profile);
    }

    public async Task<ProfileDto> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var profile = await GetOrCreateMineAsync(userId, cancellationToken);
        return Map(profile);
    }

    public async Task<ProfileDto> UpsertMineAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var profile = await GetOrCreateMineAsync(userId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? profile.Slug
                : ProfileSlug.Normalize(request.Slug);

            await EnsureSlugIsAvailableAsync(desiredSlug, profile.Id, cancellationToken);

            profile.UpdateDetails(
                displayName: request.DisplayName,
                headline: request.Headline,
                bio: request.Bio,
                location: request.Location,
                websiteUrl: request.WebsiteUrl,
                linkedInUrl: request.LinkedInUrl,
                gitHubUrl: request.GitHubUrl,
                twitterUrl: request.TwitterUrl,
                instagramUrl: request.InstagramUrl,
                slug: desiredSlug,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                profile.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                profile.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Profile"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "profile.upsert",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task<ProfileDto> PublishMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var profile = await GetOwnedProfileAsync(userId, cancellationToken);

        try
        {
            profile.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Profile"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "profile.publish",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task<ProfileDto> UnpublishMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var profile = await GetOwnedProfileAsync(userId, cancellationToken);

        profile.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "profile.unpublish",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task<ProfileDto> UploadAvatarAsync(
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        ValidateAvatar(fileName, contentType, contentLength);

        var userId = RequireCurrentUserId();
        var profile = await GetOrCreateMineAsync(userId, cancellationToken);
        var previousAvatarPath = profile.AvatarPath;

        await using var limitedStream = await CopyToLimitedMemoryStreamAsync(
            content,
            _fileStorageOptions.AvatarMaxBytes,
            cancellationToken);

        var storedPath = await _fileStorage.SaveAsync(
            limitedStream,
            fileName,
            contentType,
            cancellationToken);

        try
        {
            profile.SetAvatar(storedPath, _dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            await _fileStorage.DeleteAsync(storedPath, cancellationToken);
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Avatar"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousAvatarPath)
            && !string.Equals(previousAvatarPath, storedPath, StringComparison.OrdinalIgnoreCase))
        {
            await _fileStorage.DeleteAsync(previousAvatarPath, cancellationToken);
        }

        await _auditLogger.LogAsync(
            action: "profile.avatar.upload",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task<ProfileDto> RemoveAvatarAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var profile = await GetOrCreateMineAsync(userId, cancellationToken);
        var previousAvatarPath = profile.AvatarPath;

        profile.SetAvatar(null, _dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousAvatarPath))
        {
            await _fileStorage.DeleteAsync(previousAvatarPath, cancellationToken);
        }

        await _auditLogger.LogAsync(
            action: "profile.avatar.remove",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    private void ValidateAvatar(string fileName, string contentType, long? contentLength)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Avatar file name is required."]
            });
        }

        if (contentLength is > 0 && contentLength > _fileStorageOptions.AvatarMaxBytes)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = [$"Avatar must be at most {_fileStorageOptions.AvatarMaxBytes / 1024 / 1024} MB."]
            });
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension)
            || !_fileStorageOptions.AllowedAvatarExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Avatar extension is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedAvatarExtensions)}"
                ]
            });
        }

        if (string.IsNullOrWhiteSpace(contentType)
            || !_fileStorageOptions.AllowedAvatarContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Avatar content type is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedAvatarContentTypes)}"
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
                    ["file"] = [$"Avatar must be at most {maxBytes / 1024 / 1024} MB."]
                });
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (totalRead == 0)
        {
            await memoryStream.DisposeAsync();
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Avatar file is empty."]
            });
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<BrandProfile> GetOrCreateMineAsync(string userId, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.BrandProfiles
            .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var displayName = string.IsNullOrWhiteSpace(_currentUserService.UserName)
            ? "My Profile"
            : _currentUserService.UserName!;

        var profile = BrandProfile.Create(userId, displayName, _dateTimeProvider.UtcNow);
        profile = await EnsureUniqueSlugOnCreateAsync(profile, cancellationToken);

        _dbContext.BrandProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "profile.create",
            entityName: nameof(BrandProfile),
            entityId: profile.Id.ToString(),
            cancellationToken: cancellationToken);

        return profile;
    }

    private async Task<BrandProfile> GetOwnedProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.BrandProfiles
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("BrandProfile", userId);
        }

        return profile;
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid currentProfileId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.BrandProfiles
            .AnyAsync(
                profile => profile.Slug == slug && profile.Id != currentProfileId,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Slug '{slug}' is already in use.");
        }
    }

    private async Task<BrandProfile> EnsureUniqueSlugOnCreateAsync(
        BrandProfile profile,
        CancellationToken cancellationToken)
    {
        var baseSlug = profile.Slug;
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.BrandProfiles.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        if (!string.Equals(candidate, profile.Slug, StringComparison.Ordinal))
        {
            profile.EnsureSlug(candidate, _dateTimeProvider.UtcNow);
        }

        return profile;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private ProfileDto Map(BrandProfile profile)
    {
        var requestPath = _fileStorageOptions.RequestPath.TrimEnd('/');
        return new ProfileDto
        {
            Id = profile.Id,
            Slug = profile.Slug,
            DisplayName = profile.DisplayName,
            Headline = profile.Headline,
            Bio = profile.Bio,
            AvatarPath = profile.AvatarPath,
            AvatarUrl = string.IsNullOrWhiteSpace(profile.AvatarPath)
                ? null
                : $"{requestPath}/{profile.AvatarPath.TrimStart('/')}",
            Location = profile.Location,
            WebsiteUrl = profile.WebsiteUrl,
            LinkedInUrl = profile.LinkedInUrl,
            GitHubUrl = profile.GitHubUrl,
            TwitterUrl = profile.TwitterUrl,
            InstagramUrl = profile.InstagramUrl,
            IsPublished = profile.IsPublished,
            UpdatedAtUtc = profile.UpdatedAtUtc
        };
    }
}
