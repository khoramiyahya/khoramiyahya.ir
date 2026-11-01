using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Profiles;
using PersonalBrand.Application.Profiles.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Profiles;

internal sealed class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<UpdateProfileRequest> _updateValidator;

    public ProfileService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<UpdateProfileRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
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

    private static ProfileDto Map(BrandProfile profile) =>
        new()
        {
            Id = profile.Id,
            Slug = profile.Slug,
            DisplayName = profile.DisplayName,
            Headline = profile.Headline,
            Bio = profile.Bio,
            AvatarPath = profile.AvatarPath,
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
