using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Languages;
using PersonalBrand.Application.Languages.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Languages;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Languages;

internal sealed class LanguageService : ILanguageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateSpokenLanguageRequest> _createValidator;
    private readonly IValidator<UpdateSpokenLanguageRequest> _updateValidator;

    public LanguageService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateSpokenLanguageRequest> createValidator,
        IValidator<UpdateSpokenLanguageRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<SpokenLanguageDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var languages = await _dbContext.SpokenLanguages
            .AsNoTracking()
            .Where(language => language.IsPublished)
            .OrderByDescending(language => language.IsFeatured)
            .ThenBy(language => language.DisplayOrder)
            .ThenBy(language => language.Name)
            .ToListAsync(cancellationToken);

        return languages.Select(Map).ToArray();
    }

    public async Task<SpokenLanguageDto> GetPublishedByIdAsync(
        Guid languageId,
        CancellationToken cancellationToken = default)
    {
        var language = await _dbContext.SpokenLanguages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.Id == languageId && entry.IsPublished,
                cancellationToken);

        if (language is null)
        {
            throw new NotFoundException("SpokenLanguage", languageId);
        }

        return Map(language);
    }

    public async Task<IReadOnlyList<SpokenLanguageDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var languages = await _dbContext.SpokenLanguages
            .AsNoTracking()
            .Where(language => language.UserId == userId)
            .OrderBy(language => language.DisplayOrder)
            .ThenBy(language => language.Name)
            .ToListAsync(cancellationToken);

        return languages.Select(Map).ToArray();
    }

    public async Task<SpokenLanguageDto> CreateMineAsync(
        CreateSpokenLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        SpokenLanguage language;
        try
        {
            language = SpokenLanguage.Create(userId, request.Name, utcNow);
            language.UpdateDetails(
                name: request.Name,
                proficiency: request.Proficiency,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                language.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await EnsureUniqueNameAsync(userId, language.Name, excludeId: null, cancellationToken);

        _dbContext.SpokenLanguages.Add(language);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "language.create",
            entityName: nameof(SpokenLanguage),
            entityId: language.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(language);
    }

    public async Task<SpokenLanguageDto> UpdateMineAsync(
        Guid languageId,
        UpdateSpokenLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var language = await GetOwnedAsync(userId, languageId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            language.UpdateDetails(
                name: request.Name,
                proficiency: request.Proficiency,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                language.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                language.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await EnsureUniqueNameAsync(userId, language.Name, excludeId: language.Id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "language.update",
            entityName: nameof(SpokenLanguage),
            entityId: language.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(language);
    }

    public async Task DeleteMineAsync(Guid languageId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var language = await GetOwnedAsync(userId, languageId, cancellationToken);

        _dbContext.SpokenLanguages.Remove(language);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "language.delete",
            entityName: nameof(SpokenLanguage),
            entityId: language.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<SpokenLanguageDto> PublishMineAsync(
        Guid languageId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var language = await GetOwnedAsync(userId, languageId, cancellationToken);

        try
        {
            language.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "language.publish",
            entityName: nameof(SpokenLanguage),
            entityId: language.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(language);
    }

    public async Task<SpokenLanguageDto> UnpublishMineAsync(
        Guid languageId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var language = await GetOwnedAsync(userId, languageId, cancellationToken);

        language.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "language.unpublish",
            entityName: nameof(SpokenLanguage),
            entityId: language.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(language);
    }

    private async Task EnsureUniqueNameAsync(
        string userId,
        string name,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.SpokenLanguages.AnyAsync(
            entry => entry.UserId == userId
                && entry.Name == name
                && (!excludeId.HasValue || entry.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Name"] = ["A language with this name already exists."]
            });
        }
    }

    private async Task<SpokenLanguage> GetOwnedAsync(
        string userId,
        Guid languageId,
        CancellationToken cancellationToken)
    {
        var language = await _dbContext.SpokenLanguages
            .SingleOrDefaultAsync(
                entry => entry.Id == languageId && entry.UserId == userId,
                cancellationToken);

        if (language is null)
        {
            throw new NotFoundException("SpokenLanguage", languageId);
        }

        return language;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static SpokenLanguageDto Map(SpokenLanguage language) =>
        new()
        {
            Id = language.Id,
            Name = language.Name,
            Proficiency = language.Proficiency,
            DisplayOrder = language.DisplayOrder,
            IsFeatured = language.IsFeatured,
            IsPublished = language.IsPublished,
            UpdatedAtUtc = language.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Language"] = [exception.Message]
        });
}
