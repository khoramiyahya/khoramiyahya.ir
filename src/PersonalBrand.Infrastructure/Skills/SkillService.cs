using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Skills;
using PersonalBrand.Application.Skills.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Skills;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Skills;

internal sealed class SkillService : ISkillService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateSkillRequest> _createValidator;
    private readonly IValidator<UpdateSkillRequest> _updateValidator;

    public SkillService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateSkillRequest> createValidator,
        IValidator<UpdateSkillRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<SkillDto>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _dbContext.Skills
            .AsNoTracking()
            .Where(skill => skill.IsPublished)
            .OrderByDescending(skill => skill.IsFeatured)
            .ThenBy(skill => skill.DisplayOrder)
            .ThenByDescending(skill => skill.ProficiencyPercent)
            .ToListAsync(cancellationToken);

        return skills.Select(Map).ToArray();
    }

    public async Task<SkillDto> GetPublishedByIdAsync(
        Guid skillId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.Skills
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == skillId && item.IsPublished,
                cancellationToken);

        if (skill is null)
        {
            throw new NotFoundException("Skill", skillId);
        }

        return Map(skill);
    }

    public async Task<IReadOnlyList<SkillDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var skills = await _dbContext.Skills
            .AsNoTracking()
            .Where(skill => skill.UserId == userId)
            .OrderBy(skill => skill.DisplayOrder)
            .ThenBy(skill => skill.Name)
            .ToListAsync(cancellationToken);

        return skills.Select(Map).ToArray();
    }

    public async Task<SkillDto> CreateMineAsync(
        CreateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Skill skill;
        try
        {
            skill = Skill.Create(userId, request.Name, utcNow);
            skill.UpdateDetails(
                name: request.Name,
                category: request.Category,
                description: request.Description,
                proficiencyPercent: request.ProficiencyPercent,
                yearsExperience: request.YearsExperience,
                iconName: request.IconName,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                skill.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Skills.Add(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "skill.create",
            entityName: nameof(Skill),
            entityId: skill.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(skill);
    }

    public async Task<SkillDto> UpdateMineAsync(
        Guid skillId,
        UpdateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var skill = await GetOwnedAsync(userId, skillId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            skill.UpdateDetails(
                name: request.Name,
                category: request.Category,
                description: request.Description,
                proficiencyPercent: request.ProficiencyPercent,
                yearsExperience: request.YearsExperience,
                iconName: request.IconName,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                skill.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                skill.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "skill.update",
            entityName: nameof(Skill),
            entityId: skill.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(skill);
    }

    public async Task DeleteMineAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var skill = await GetOwnedAsync(userId, skillId, cancellationToken);

        _dbContext.Skills.Remove(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "skill.delete",
            entityName: nameof(Skill),
            entityId: skill.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<SkillDto> PublishMineAsync(
        Guid skillId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var skill = await GetOwnedAsync(userId, skillId, cancellationToken);

        try
        {
            skill.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "skill.publish",
            entityName: nameof(Skill),
            entityId: skill.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(skill);
    }

    public async Task<SkillDto> UnpublishMineAsync(
        Guid skillId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var skill = await GetOwnedAsync(userId, skillId, cancellationToken);

        skill.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "skill.unpublish",
            entityName: nameof(Skill),
            entityId: skill.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(skill);
    }

    private async Task<Skill> GetOwnedAsync(
        string userId,
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var skill = await _dbContext.Skills
            .SingleOrDefaultAsync(
                item => item.Id == skillId && item.UserId == userId,
                cancellationToken);

        if (skill is null)
        {
            throw new NotFoundException("Skill", skillId);
        }

        return skill;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static SkillDto Map(Skill skill) =>
        new()
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            Description = skill.Description,
            ProficiencyPercent = skill.ProficiencyPercent,
            YearsExperience = skill.YearsExperience,
            IconName = skill.IconName,
            DisplayOrder = skill.DisplayOrder,
            IsFeatured = skill.IsFeatured,
            IsPublished = skill.IsPublished,
            UpdatedAtUtc = skill.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Skill"] = [exception.Message]
        });
}
