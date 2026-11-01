using PersonalBrand.Application.Publications.Contracts;

namespace PersonalBrand.Application.Publications;

/// <summary>
/// Publication use cases.
/// </summary>
public interface IPublicationService
{
    Task<IReadOnlyList<PublicationDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<PublicationDto> GetPublishedByIdAsync(Guid publicationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicationDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<PublicationDto> CreateMineAsync(CreatePublicationRequest request, CancellationToken cancellationToken = default);

    Task<PublicationDto> UpdateMineAsync(
        Guid publicationId,
        UpdatePublicationRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid publicationId, CancellationToken cancellationToken = default);

    Task<PublicationDto> PublishMineAsync(Guid publicationId, CancellationToken cancellationToken = default);

    Task<PublicationDto> UnpublishMineAsync(Guid publicationId, CancellationToken cancellationToken = default);
}
