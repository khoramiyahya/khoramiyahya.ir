using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Experience;
using PersonalBrand.Application.Experience.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Experience;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Experience;

internal sealed class ExperienceService : IExperienceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateWorkExperienceRequest> _createValidator;
    private readonly IValidator<UpdateWorkExperienceRequest> _updateValidator;

    public ExperienceService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateWorkExperienceRequest> createValidator,
        IValidator<UpdateWorkExperienceRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<WorkExperienceDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var experiences = await _dbContext.WorkExperiences
            .AsNoTracking()
            .Where(experience => experience.IsPublished)
            .OrderByDescending(experience => experience.IsFeatured)
            .ThenBy(experience => experience.DisplayOrder)
            .ThenByDescending(experience => experience.StartedOn)
            .ToListAsync(cancellationToken);

        return experiences.Select(Map).ToArray();
    }

    public async Task<WorkExperienceDto> GetPublishedByIdAsync(
        Guid experienceId,
        CancellationToken cancellationToken = default)
    {
        var experience = await _dbContext.WorkExperiences
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == experienceId && item.IsPublished,
                cancellationToken);

        if (experience is null)
        {
            throw new NotFoundException("WorkExperience", experienceId);
        }

        return Map(experience);
    }

    public async Task<IReadOnlyList<WorkExperienceDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var experiences = await _dbContext.WorkExperiences
            .AsNoTracking()
            .Where(experience => experience.UserId == userId)
            .OrderBy(experience => experience.DisplayOrder)
            .ThenByDescending(experience => experience.StartedOn)
            .ToListAsync(cancellationToken);

        return experiences.Select(Map).ToArray();
    }

    public async Task<WorkExperienceDto> CreateMineAsync(
        CreateWorkExperienceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        WorkExperience experience;
        try
        {
            experience = WorkExperience.Create(
                userId,
                request.Company,
                request.Title,
                request.StartedOn,
                utcNow);

            experience.UpdateDetails(
                company: request.Company,
                title: request.Title,
                location: request.Location,
                description: request.Description,
                companyUrl: request.CompanyUrl,
                startedOn: request.StartedOn,
                endedOn: request.EndedOn,
                isCurrent: request.IsCurrent,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                experience.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.WorkExperiences.Add(experience);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "experience.create",
            entityName: nameof(WorkExperience),
            entityId: experience.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(experience);
    }

    public async Task<WorkExperienceDto> UpdateMineAsync(
        Guid experienceId,
        UpdateWorkExperienceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var experience = await GetOwnedAsync(userId, experienceId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            experience.UpdateDetails(
                company: request.Company,
                title: request.Title,
                location: request.Location,
                description: request.Description,
                companyUrl: request.CompanyUrl,
                startedOn: request.StartedOn,
                endedOn: request.EndedOn,
                isCurrent: request.IsCurrent,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                experience.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                experience.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "experience.update",
            entityName: nameof(WorkExperience),
            entityId: experience.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(experience);
    }

    public async Task DeleteMineAsync(Guid experienceId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var experience = await GetOwnedAsync(userId, experienceId, cancellationToken);

        _dbContext.WorkExperiences.Remove(experience);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "experience.delete",
            entityName: nameof(WorkExperience),
            entityId: experience.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<WorkExperienceDto> PublishMineAsync(
        Guid experienceId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var experience = await GetOwnedAsync(userId, experienceId, cancellationToken);

        try
        {
            experience.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "experience.publish",
            entityName: nameof(WorkExperience),
            entityId: experience.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(experience);
    }

    public async Task<WorkExperienceDto> UnpublishMineAsync(
        Guid experienceId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var experience = await GetOwnedAsync(userId, experienceId, cancellationToken);

        experience.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "experience.unpublish",
            entityName: nameof(WorkExperience),
            entityId: experience.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(experience);
    }

    private async Task<WorkExperience> GetOwnedAsync(
        string userId,
        Guid experienceId,
        CancellationToken cancellationToken)
    {
        var experience = await _dbContext.WorkExperiences
            .SingleOrDefaultAsync(
                item => item.Id == experienceId && item.UserId == userId,
                cancellationToken);

        if (experience is null)
        {
            throw new NotFoundException("WorkExperience", experienceId);
        }

        return experience;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static WorkExperienceDto Map(WorkExperience experience) =>
        new()
        {
            Id = experience.Id,
            Company = experience.Company,
            Title = experience.Title,
            Location = experience.Location,
            Description = experience.Description,
            CompanyUrl = experience.CompanyUrl,
            StartedOn = experience.StartedOn,
            EndedOn = experience.EndedOn,
            IsCurrent = experience.IsCurrent,
            DisplayOrder = experience.DisplayOrder,
            IsFeatured = experience.IsFeatured,
            IsPublished = experience.IsPublished,
            UpdatedAtUtc = experience.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Experience"] = [exception.Message]
        });
}
