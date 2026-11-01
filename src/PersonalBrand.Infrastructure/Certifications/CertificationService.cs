using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Certifications;
using PersonalBrand.Application.Certifications.Contracts;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Domain.Certifications;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Certifications;

internal sealed class CertificationService : ICertificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateCertificationRequest> _createValidator;
    private readonly IValidator<UpdateCertificationRequest> _updateValidator;

    public CertificationService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateCertificationRequest> createValidator,
        IValidator<UpdateCertificationRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<CertificationDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var certifications = await _dbContext.Certifications
            .AsNoTracking()
            .Where(certification => certification.IsPublished)
            .OrderByDescending(certification => certification.IsFeatured)
            .ThenBy(certification => certification.DisplayOrder)
            .ThenByDescending(certification => certification.IssuedOn)
            .ToListAsync(cancellationToken);

        return certifications.Select(Map).ToArray();
    }

    public async Task<CertificationDto> GetPublishedByIdAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        var certification = await _dbContext.Certifications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == certificationId && item.IsPublished,
                cancellationToken);

        if (certification is null)
        {
            throw new NotFoundException("Certification", certificationId);
        }

        return Map(certification);
    }

    public async Task<IReadOnlyList<CertificationDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var certifications = await _dbContext.Certifications
            .AsNoTracking()
            .Where(certification => certification.UserId == userId)
            .OrderBy(certification => certification.DisplayOrder)
            .ThenByDescending(certification => certification.IssuedOn)
            .ToListAsync(cancellationToken);

        return certifications.Select(Map).ToArray();
    }

    public async Task<CertificationDto> CreateMineAsync(
        CreateCertificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Certification certification;
        try
        {
            certification = Certification.Create(
                userId,
                request.Name,
                request.Issuer,
                request.IssuedOn,
                utcNow);

            certification.UpdateDetails(
                name: request.Name,
                issuer: request.Issuer,
                credentialId: request.CredentialId,
                credentialUrl: request.CredentialUrl,
                description: request.Description,
                issuedOn: request.IssuedOn,
                expiresOn: request.ExpiresOn,
                doesNotExpire: request.DoesNotExpire,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                certification.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Certifications.Add(certification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "certification.create",
            entityName: nameof(Certification),
            entityId: certification.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(certification);
    }

    public async Task<CertificationDto> UpdateMineAsync(
        Guid certificationId,
        UpdateCertificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var certification = await GetOwnedAsync(userId, certificationId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            certification.UpdateDetails(
                name: request.Name,
                issuer: request.Issuer,
                credentialId: request.CredentialId,
                credentialUrl: request.CredentialUrl,
                description: request.Description,
                issuedOn: request.IssuedOn,
                expiresOn: request.ExpiresOn,
                doesNotExpire: request.DoesNotExpire,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                certification.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                certification.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "certification.update",
            entityName: nameof(Certification),
            entityId: certification.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(certification);
    }

    public async Task DeleteMineAsync(Guid certificationId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var certification = await GetOwnedAsync(userId, certificationId, cancellationToken);

        _dbContext.Certifications.Remove(certification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "certification.delete",
            entityName: nameof(Certification),
            entityId: certification.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<CertificationDto> PublishMineAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var certification = await GetOwnedAsync(userId, certificationId, cancellationToken);

        try
        {
            certification.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "certification.publish",
            entityName: nameof(Certification),
            entityId: certification.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(certification);
    }

    public async Task<CertificationDto> UnpublishMineAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var certification = await GetOwnedAsync(userId, certificationId, cancellationToken);

        certification.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "certification.unpublish",
            entityName: nameof(Certification),
            entityId: certification.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(certification);
    }

    private async Task<Certification> GetOwnedAsync(
        string userId,
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var certification = await _dbContext.Certifications
            .SingleOrDefaultAsync(
                item => item.Id == certificationId && item.UserId == userId,
                cancellationToken);

        if (certification is null)
        {
            throw new NotFoundException("Certification", certificationId);
        }

        return certification;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static CertificationDto Map(Certification certification) =>
        new()
        {
            Id = certification.Id,
            Name = certification.Name,
            Issuer = certification.Issuer,
            CredentialId = certification.CredentialId,
            CredentialUrl = certification.CredentialUrl,
            Description = certification.Description,
            IssuedOn = certification.IssuedOn,
            ExpiresOn = certification.ExpiresOn,
            DoesNotExpire = certification.DoesNotExpire,
            DisplayOrder = certification.DisplayOrder,
            IsFeatured = certification.IsFeatured,
            IsPublished = certification.IsPublished,
            UpdatedAtUtc = certification.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Certification"] = [exception.Message]
        });
}
