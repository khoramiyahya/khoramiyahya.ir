using PersonalBrand.Application.Partners.Contracts;

namespace PersonalBrand.Application.Partners;

/// <summary>
/// Partner and client logo use cases.
/// </summary>
public interface IPartnerService
{
    Task<IReadOnlyList<PartnerDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<PartnerDto> GetPublishedByIdAsync(Guid partnerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<PartnerDto> CreateMineAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default);

    Task<PartnerDto> UpdateMineAsync(
        Guid partnerId,
        UpdatePartnerRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid partnerId, CancellationToken cancellationToken = default);

    Task<PartnerDto> PublishMineAsync(Guid partnerId, CancellationToken cancellationToken = default);

    Task<PartnerDto> UnpublishMineAsync(Guid partnerId, CancellationToken cancellationToken = default);
}
