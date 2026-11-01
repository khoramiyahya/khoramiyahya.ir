using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Settings;
using PersonalBrand.Application.Settings.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Settings;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Settings;

internal sealed class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<UpdateSiteSettingsRequest> _updateValidator;

    public SiteSettingsService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<UpdateSiteSettingsRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _updateValidator = updateValidator;
    }

    public async Task<SiteSettingsDto> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateAsync(cancellationToken);
        return Map(settings);
    }

    public async Task<SiteSettingsDto> GetMineAsync(CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        var settings = await GetOrCreateAsync(cancellationToken);
        return Map(settings);
    }

    public async Task<SiteSettingsDto> UpdateMineAsync(
        UpdateSiteSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAuthenticatedUser();
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var settings = await GetOrCreateAsync(cancellationToken);

        try
        {
            settings.Update(
                siteName: request.SiteName,
                tagline: request.Tagline,
                metaTitle: request.MetaTitle,
                metaDescription: request.MetaDescription,
                metaKeywords: request.MetaKeywords,
                canonicalBaseUrl: request.CanonicalBaseUrl,
                ogImageUrl: request.OgImageUrl,
                faviconUrl: request.FaviconUrl,
                primaryEmail: request.PrimaryEmail,
                primaryPhone: request.PrimaryPhone,
                address: request.Address,
                googleAnalyticsId: request.GoogleAnalyticsId,
                isMaintenanceMode: request.IsMaintenanceMode,
                utcNow: _dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Settings"] = [exception.Message]
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "settings.update",
            entityName: nameof(SiteSettings),
            entityId: settings.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(settings);
    }

    private async Task<SiteSettings> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.SiteSettings
            .SingleOrDefaultAsync(settings => settings.Key == SiteSettings.SingletonKey, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var created = SiteSettings.CreateDefault(_dateTimeProvider.UtcNow);
        _dbContext.SiteSettings.Add(created);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    private void RequireAuthenticatedUser()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }
    }

    private static SiteSettingsDto Map(SiteSettings settings) =>
        new()
        {
            SiteName = settings.SiteName,
            Tagline = settings.Tagline,
            MetaTitle = settings.MetaTitle,
            MetaDescription = settings.MetaDescription,
            MetaKeywords = settings.MetaKeywords,
            CanonicalBaseUrl = settings.CanonicalBaseUrl,
            OgImageUrl = settings.OgImageUrl,
            FaviconUrl = settings.FaviconUrl,
            PrimaryEmail = settings.PrimaryEmail,
            PrimaryPhone = settings.PrimaryPhone,
            Address = settings.Address,
            GoogleAnalyticsId = settings.GoogleAnalyticsId,
            IsMaintenanceMode = settings.IsMaintenanceMode,
            UpdatedAtUtc = settings.UpdatedAtUtc
        };
}
