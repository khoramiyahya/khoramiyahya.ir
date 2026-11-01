using PersonalBrand.Application.Profiles.Contracts;

namespace PersonalBrand.Application.Profiles;

/// <summary>
/// Brand profile use cases.
/// </summary>
public interface IProfileService
{
    Task<ProfileDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<ProfileDto> GetMineAsync(CancellationToken cancellationToken = default);

    Task<ProfileDto> UpsertMineAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task<ProfileDto> PublishMineAsync(CancellationToken cancellationToken = default);

    Task<ProfileDto> UnpublishMineAsync(CancellationToken cancellationToken = default);
}
