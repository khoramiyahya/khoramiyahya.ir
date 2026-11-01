using PersonalBrand.Application.Services.Contracts;

namespace PersonalBrand.Application.Services;

/// <summary>
/// Service catalog use cases.
/// </summary>
public interface IServiceOfferService
{
    Task<IReadOnlyList<ServiceOfferDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<ServiceOfferDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceOfferDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<ServiceOfferDto> CreateMineAsync(CreateServiceOfferRequest request, CancellationToken cancellationToken = default);

    Task<ServiceOfferDto> UpdateMineAsync(Guid serviceId, UpdateServiceOfferRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<ServiceOfferDto> PublishMineAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<ServiceOfferDto> UnpublishMineAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
