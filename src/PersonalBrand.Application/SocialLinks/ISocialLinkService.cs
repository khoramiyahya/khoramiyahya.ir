using PersonalBrand.Application.SocialLinks.Contracts;

namespace PersonalBrand.Application.SocialLinks;

/// <summary>
/// Social link use cases.
/// </summary>
public interface ISocialLinkService
{
    Task<IReadOnlyList<SocialLinkDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<SocialLinkDto> GetPublishedByIdAsync(Guid socialLinkId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SocialLinkDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<SocialLinkDto> CreateMineAsync(CreateSocialLinkRequest request, CancellationToken cancellationToken = default);

    Task<SocialLinkDto> UpdateMineAsync(Guid socialLinkId, UpdateSocialLinkRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid socialLinkId, CancellationToken cancellationToken = default);

    Task<SocialLinkDto> PublishMineAsync(Guid socialLinkId, CancellationToken cancellationToken = default);

    Task<SocialLinkDto> UnpublishMineAsync(Guid socialLinkId, CancellationToken cancellationToken = default);
}
