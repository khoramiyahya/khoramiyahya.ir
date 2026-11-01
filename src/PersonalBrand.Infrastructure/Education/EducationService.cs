using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Education;
using PersonalBrand.Application.Education.Contracts;
using PersonalBrand.Domain.Education;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Education;

internal sealed class EducationService : IEducationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateEducationEntryRequest> _createValidator;
    private readonly IValidator<UpdateEducationEntryRequest> _updateValidator;

    public EducationService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateEducationEntryRequest> createValidator,
        IValidator<UpdateEducationEntryRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<EducationEntryDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var entries = await _dbContext.EducationEntries
            .AsNoTracking()
            .Where(entry => entry.IsPublished)
            .OrderByDescending(entry => entry.IsFeatured)
            .ThenBy(entry => entry.DisplayOrder)
            .ThenByDescending(entry => entry.StartedOn)
            .ToListAsync(cancellationToken);

        return entries.Select(Map).ToArray();
    }

    public async Task<EducationEntryDto> GetPublishedByIdAsync(
        Guid educationId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.EducationEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == educationId && item.IsPublished,
                cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException("EducationEntry", educationId);
        }

        return Map(entry);
    }

    public async Task<IReadOnlyList<EducationEntryDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var entries = await _dbContext.EducationEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .OrderBy(entry => entry.DisplayOrder)
            .ThenByDescending(entry => entry.StartedOn)
            .ToListAsync(cancellationToken);

        return entries.Select(Map).ToArray();
    }

    public async Task<EducationEntryDto> CreateMineAsync(
        CreateEducationEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        EducationEntry entry;
        try
        {
            entry = EducationEntry.Create(
                userId,
                request.Institution,
                request.Degree,
                request.StartedOn,
                utcNow);

            entry.UpdateDetails(
                institution: request.Institution,
                degree: request.Degree,
                fieldOfStudy: request.FieldOfStudy,
                location: request.Location,
                description: request.Description,
                institutionUrl: request.InstitutionUrl,
                startedOn: request.StartedOn,
                endedOn: request.EndedOn,
                isCurrent: request.IsCurrent,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                entry.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.EducationEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "education.create",
            entityName: nameof(EducationEntry),
            entityId: entry.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(entry);
    }

    public async Task<EducationEntryDto> UpdateMineAsync(
        Guid educationId,
        UpdateEducationEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var entry = await GetOwnedAsync(userId, educationId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            entry.UpdateDetails(
                institution: request.Institution,
                degree: request.Degree,
                fieldOfStudy: request.FieldOfStudy,
                location: request.Location,
                description: request.Description,
                institutionUrl: request.InstitutionUrl,
                startedOn: request.StartedOn,
                endedOn: request.EndedOn,
                isCurrent: request.IsCurrent,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                entry.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                entry.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "education.update",
            entityName: nameof(EducationEntry),
            entityId: entry.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(entry);
    }

    public async Task DeleteMineAsync(Guid educationId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var entry = await GetOwnedAsync(userId, educationId, cancellationToken);

        _dbContext.EducationEntries.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "education.delete",
            entityName: nameof(EducationEntry),
            entityId: entry.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<EducationEntryDto> PublishMineAsync(
        Guid educationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var entry = await GetOwnedAsync(userId, educationId, cancellationToken);

        try
        {
            entry.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "education.publish",
            entityName: nameof(EducationEntry),
            entityId: entry.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(entry);
    }

    public async Task<EducationEntryDto> UnpublishMineAsync(
        Guid educationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var entry = await GetOwnedAsync(userId, educationId, cancellationToken);

        entry.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "education.unpublish",
            entityName: nameof(EducationEntry),
            entityId: entry.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(entry);
    }

    private async Task<EducationEntry> GetOwnedAsync(
        string userId,
        Guid educationId,
        CancellationToken cancellationToken)
    {
        var entry = await _dbContext.EducationEntries
            .SingleOrDefaultAsync(
                item => item.Id == educationId && item.UserId == userId,
                cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException("EducationEntry", educationId);
        }

        return entry;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static EducationEntryDto Map(EducationEntry entry) =>
        new()
        {
            Id = entry.Id,
            Institution = entry.Institution,
            Degree = entry.Degree,
            FieldOfStudy = entry.FieldOfStudy,
            Location = entry.Location,
            Description = entry.Description,
            InstitutionUrl = entry.InstitutionUrl,
            StartedOn = entry.StartedOn,
            EndedOn = entry.EndedOn,
            IsCurrent = entry.IsCurrent,
            DisplayOrder = entry.DisplayOrder,
            IsFeatured = entry.IsFeatured,
            IsPublished = entry.IsPublished,
            UpdatedAtUtc = entry.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Education"] = [exception.Message]
        });
}
