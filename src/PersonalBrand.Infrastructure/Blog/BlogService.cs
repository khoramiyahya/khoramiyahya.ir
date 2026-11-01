using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Blog;
using PersonalBrand.Application.Blog.Contracts;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Domain.Blog;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Blog;

internal sealed class BlogService : IBlogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IFileStorage _fileStorage;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IValidator<CreateBlogPostRequest> _createValidator;
    private readonly IValidator<UpdateBlogPostRequest> _updateValidator;

    public BlogService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IFileStorage fileStorage,
        IOptions<FileStorageOptions> fileStorageOptions,
        IValidator<CreateBlogPostRequest> createValidator,
        IValidator<UpdateBlogPostRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _fileStorage = fileStorage;
        _fileStorageOptions = fileStorageOptions.Value;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<BlogPostDto>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        var posts = await _dbContext.BlogPosts
            .AsNoTracking()
            .Where(post => post.IsPublished)
            .OrderByDescending(post => post.IsFeatured)
            .ThenByDescending(post => post.PublishedAtUtc)
            .ThenByDescending(post => post.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return posts.Select(Map).ToArray();
    }

    public async Task<BlogPostDto> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException("Published blog post was not found.");
        }

        string normalizedSlug;
        try
        {
            normalizedSlug = ProfileSlug.Normalize(slug);
        }
        catch (DomainException)
        {
            throw new NotFoundException("Published blog post was not found.");
        }

        var post = await _dbContext.BlogPosts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug && item.IsPublished,
                cancellationToken);

        if (post is null)
        {
            throw new NotFoundException("BlogPost", normalizedSlug);
        }

        return Map(post);
    }

    public async Task<IReadOnlyList<BlogPostDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var posts = await _dbContext.BlogPosts
            .AsNoTracking()
            .Where(post => post.UserId == userId)
            .OrderByDescending(post => post.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return posts.Select(Map).ToArray();
    }

    public async Task<BlogPostDto> CreateMineAsync(
        CreateBlogPostRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        BlogPost post;
        try
        {
            post = BlogPost.Create(userId, request.Title, utcNow, request.Slug);
            await EnsureUniqueSlugAsync(post, cancellationToken);

            post.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                body: request.Body,
                tags: request.Tags,
                isFeatured: request.IsFeatured,
                slug: null,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                post.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.BlogPosts.Add(post);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "blog.create",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    public async Task<BlogPostDto> UpdateMineAsync(
        Guid postId,
        UpdateBlogPostRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? post.Slug
                : ProfileSlug.Normalize(request.Slug);

            await EnsureSlugIsAvailableAsync(desiredSlug, post.Id, cancellationToken);

            post.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                body: request.Body,
                tags: request.Tags,
                isFeatured: request.IsFeatured,
                slug: desiredSlug,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                post.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                post.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "blog.update",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    public async Task DeleteMineAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);
        var coverImagePath = post.CoverImagePath;

        _dbContext.BlogPosts.Remove(post);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(coverImagePath))
        {
            await _fileStorage.DeleteAsync(coverImagePath, cancellationToken);
        }

        await _auditLogger.LogAsync(
            action: "blog.delete",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<BlogPostDto> PublishMineAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);

        try
        {
            post.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "blog.publish",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    public async Task<BlogPostDto> UnpublishMineAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);

        post.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "blog.unpublish",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    public async Task<BlogPostDto> UploadCoverAsync(
        Guid postId,
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        ValidateCover(fileName, contentType, contentLength);

        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);
        var previousCoverPath = post.CoverImagePath;

        await using var limitedStream = await CopyToLimitedMemoryStreamAsync(
            content,
            _fileStorageOptions.CoverMaxBytes,
            cancellationToken);

        var storedPath = await _fileStorage.SaveAsync(
            limitedStream,
            fileName,
            contentType,
            cancellationToken);

        try
        {
            post.SetCoverImage(storedPath, _dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            await _fileStorage.DeleteAsync(storedPath, cancellationToken);
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Cover"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousCoverPath)
            && !string.Equals(previousCoverPath, storedPath, StringComparison.OrdinalIgnoreCase))
        {
            await _fileStorage.DeleteAsync(previousCoverPath, cancellationToken);
        }

        await _auditLogger.LogAsync(
            action: "blog.cover.upload",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    public async Task<BlogPostDto> RemoveCoverAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var post = await GetOwnedPostAsync(userId, postId, cancellationToken);
        var previousCoverPath = post.CoverImagePath;

        post.SetCoverImage(null, _dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousCoverPath))
        {
            await _fileStorage.DeleteAsync(previousCoverPath, cancellationToken);
        }

        await _auditLogger.LogAsync(
            action: "blog.cover.remove",
            entityName: nameof(BlogPost),
            entityId: post.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(post);
    }

    private void ValidateCover(string fileName, string contentType, long? contentLength)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Cover file name is required."]
            });
        }

        if (contentLength is > 0 && contentLength > _fileStorageOptions.CoverMaxBytes)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = [$"Cover must be at most {_fileStorageOptions.CoverMaxBytes / 1024 / 1024} MB."]
            });
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension)
            || !_fileStorageOptions.AllowedCoverExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Cover extension is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedCoverExtensions)}"
                ]
            });
        }

        if (string.IsNullOrWhiteSpace(contentType)
            || !_fileStorageOptions.AllowedCoverContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] =
                [
                    $"Cover content type is not allowed. Allowed: {string.Join(", ", _fileStorageOptions.AllowedCoverContentTypes)}"
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
                    ["file"] = [$"Cover must be at most {maxBytes / 1024 / 1024} MB."]
                });
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (totalRead == 0)
        {
            await memoryStream.DisposeAsync();
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Cover file is empty."]
            });
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task EnsureUniqueSlugAsync(BlogPost post, CancellationToken cancellationToken)
    {
        var baseSlug = post.Slug;
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.BlogPosts.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        if (!string.Equals(candidate, post.Slug, StringComparison.Ordinal))
        {
            post.EnsureSlug(candidate, _dateTimeProvider.UtcNow);
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid currentPostId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.BlogPosts
            .AnyAsync(
                post => post.Slug == slug && post.Id != currentPostId,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Slug '{slug}' is already in use.");
        }
    }

    private async Task<BlogPost> GetOwnedPostAsync(
        string userId,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var post = await _dbContext.BlogPosts
            .SingleOrDefaultAsync(
                item => item.Id == postId && item.UserId == userId,
                cancellationToken);

        if (post is null)
        {
            throw new NotFoundException("BlogPost", postId);
        }

        return post;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private BlogPostDto Map(BlogPost post)
    {
        var requestPath = _fileStorageOptions.RequestPath.TrimEnd('/');
        return new BlogPostDto
        {
            Id = post.Id,
            Slug = post.Slug,
            Title = post.Title,
            Summary = post.Summary,
            Body = post.Body,
            CoverImagePath = post.CoverImagePath,
            CoverImageUrl = string.IsNullOrWhiteSpace(post.CoverImagePath)
                ? null
                : $"{requestPath}/{post.CoverImagePath.TrimStart('/')}",
            Tags = post.Tags,
            IsFeatured = post.IsFeatured,
            IsPublished = post.IsPublished,
            PublishedAtUtc = post.PublishedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc
        };
    }

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["BlogPost"] = [exception.Message]
        });
}
