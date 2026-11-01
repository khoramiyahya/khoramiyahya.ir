using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Projects;
using PersonalBrand.Application.Projects.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Domain.Projects;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Projects;

internal sealed class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IValidator<CreateProjectRequest> _createValidator;
    private readonly IValidator<UpdateProjectRequest> _updateValidator;

    public ProjectService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IOptions<FileStorageOptions> fileStorageOptions,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _fileStorageOptions = fileStorageOptions.Value;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _dbContext.PortfolioProjects
            .AsNoTracking()
            .Where(project => project.IsPublished)
            .OrderByDescending(project => project.IsFeatured)
            .ThenBy(project => project.DisplayOrder)
            .ThenByDescending(project => project.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return projects.Select(Map).ToArray();
    }

    public async Task<ProjectDto> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundException("Published project was not found.");
        }

        string normalizedSlug;
        try
        {
            normalizedSlug = ProfileSlug.Normalize(slug);
        }
        catch (DomainException)
        {
            throw new NotFoundException("Published project was not found.");
        }

        var project = await _dbContext.PortfolioProjects
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug && item.IsPublished,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("PortfolioProject", normalizedSlug);
        }

        return Map(project);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var projects = await _dbContext.PortfolioProjects
            .AsNoTracking()
            .Where(project => project.UserId == userId)
            .OrderBy(project => project.DisplayOrder)
            .ThenByDescending(project => project.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return projects.Select(Map).ToArray();
    }

    public async Task<ProjectDto> CreateMineAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        PortfolioProject project;
        try
        {
            project = PortfolioProject.Create(userId, request.Title, utcNow, request.Slug);
            await EnsureUniqueSlugAsync(project, cancellationToken);

            project.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                description: request.Description,
                projectUrl: request.ProjectUrl,
                repoUrl: request.RepoUrl,
                technologies: request.Technologies,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                startedOn: request.StartedOn,
                completedOn: request.CompletedOn,
                slug: null,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                project.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.PortfolioProjects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "project.create",
            entityName: nameof(PortfolioProject),
            entityId: project.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(project);
    }

    public async Task<ProjectDto> UpdateMineAsync(
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var project = await GetOwnedProjectAsync(userId, projectId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            var desiredSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? project.Slug
                : ProfileSlug.Normalize(request.Slug);

            await EnsureSlugIsAvailableAsync(desiredSlug, project.Id, cancellationToken);

            project.UpdateDetails(
                title: request.Title,
                summary: request.Summary,
                description: request.Description,
                projectUrl: request.ProjectUrl,
                repoUrl: request.RepoUrl,
                technologies: request.Technologies,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                startedOn: request.StartedOn,
                completedOn: request.CompletedOn,
                slug: desiredSlug,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                project.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                project.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "project.update",
            entityName: nameof(PortfolioProject),
            entityId: project.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(project);
    }

    public async Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var project = await GetOwnedProjectAsync(userId, projectId, cancellationToken);

        _dbContext.PortfolioProjects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "project.delete",
            entityName: nameof(PortfolioProject),
            entityId: project.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<ProjectDto> PublishMineAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var project = await GetOwnedProjectAsync(userId, projectId, cancellationToken);

        try
        {
            project.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "project.publish",
            entityName: nameof(PortfolioProject),
            entityId: project.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(project);
    }

    public async Task<ProjectDto> UnpublishMineAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var project = await GetOwnedProjectAsync(userId, projectId, cancellationToken);

        project.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "project.unpublish",
            entityName: nameof(PortfolioProject),
            entityId: project.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(project);
    }

    private async Task EnsureUniqueSlugAsync(PortfolioProject project, CancellationToken cancellationToken)
    {
        var baseSlug = project.Slug;
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.PortfolioProjects.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        if (!string.Equals(candidate, project.Slug, StringComparison.Ordinal))
        {
            project.EnsureSlug(candidate, _dateTimeProvider.UtcNow);
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        Guid currentProjectId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PortfolioProjects
            .AnyAsync(
                project => project.Slug == slug && project.Id != currentProjectId,
                cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Slug '{slug}' is already in use.");
        }
    }

    private async Task<PortfolioProject> GetOwnedProjectAsync(
        string userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await _dbContext.PortfolioProjects
            .SingleOrDefaultAsync(
                item => item.Id == projectId && item.UserId == userId,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("PortfolioProject", projectId);
        }

        return project;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private ProjectDto Map(PortfolioProject project)
    {
        var requestPath = _fileStorageOptions.RequestPath.TrimEnd('/');
        return new ProjectDto
        {
            Id = project.Id,
            Slug = project.Slug,
            Title = project.Title,
            Summary = project.Summary,
            Description = project.Description,
            CoverImagePath = project.CoverImagePath,
            CoverImageUrl = string.IsNullOrWhiteSpace(project.CoverImagePath)
                ? null
                : $"{requestPath}/{project.CoverImagePath.TrimStart('/')}",
            ProjectUrl = project.ProjectUrl,
            RepoUrl = project.RepoUrl,
            Technologies = project.Technologies,
            DisplayOrder = project.DisplayOrder,
            IsFeatured = project.IsFeatured,
            IsPublished = project.IsPublished,
            StartedOn = project.StartedOn,
            CompletedOn = project.CompletedOn,
            UpdatedAtUtc = project.UpdatedAtUtc
        };
    }

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Project"] = [exception.Message]
        });
}
