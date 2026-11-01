using PersonalBrand.Application.Awards.Contracts;

namespace PersonalBrand.Application.Awards;

/// <summary>
/// Award use cases.
/// </summary>
public interface IAwardService
{
    Task<IReadOnlyList<AwardDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<AwardDto> GetPublishedByIdAsync(Guid awardId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AwardDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<AwardDto> CreateMineAsync(CreateAwardRequest request, CancellationToken cancellationToken = default);

    Task<AwardDto> UpdateMineAsync(
        Guid awardId,
        UpdateAwardRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid awardId, CancellationToken cancellationToken = default);

    Task<AwardDto> PublishMineAsync(Guid awardId, CancellationToken cancellationToken = default);

    Task<AwardDto> UnpublishMineAsync(Guid awardId, CancellationToken cancellationToken = default);
}
