using PersonalBrand.Application.Certifications.Contracts;

namespace PersonalBrand.Application.Certifications;

/// <summary>
/// Certification use cases.
/// </summary>
public interface ICertificationService
{
    Task<IReadOnlyList<CertificationDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<CertificationDto> GetPublishedByIdAsync(Guid certificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CertificationDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<CertificationDto> CreateMineAsync(CreateCertificationRequest request, CancellationToken cancellationToken = default);

    Task<CertificationDto> UpdateMineAsync(Guid certificationId, UpdateCertificationRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid certificationId, CancellationToken cancellationToken = default);

    Task<CertificationDto> PublishMineAsync(Guid certificationId, CancellationToken cancellationToken = default);

    Task<CertificationDto> UnpublishMineAsync(Guid certificationId, CancellationToken cancellationToken = default);
}
