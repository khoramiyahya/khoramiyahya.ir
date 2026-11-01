using PersonalBrand.Application.Settings.Contracts;

namespace PersonalBrand.Application.Settings;

/// <summary>
/// Site settings use cases.
/// </summary>
public interface ISiteSettingsService
{
    Task<SiteSettingsDto> GetPublicAsync(CancellationToken cancellationToken = default);

    Task<SiteSettingsDto> GetMineAsync(CancellationToken cancellationToken = default);

    Task<SiteSettingsDto> UpdateMineAsync(
        UpdateSiteSettingsRequest request,
        CancellationToken cancellationToken = default);
}
