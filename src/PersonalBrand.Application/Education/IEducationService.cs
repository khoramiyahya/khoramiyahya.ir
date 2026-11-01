using PersonalBrand.Application.Education.Contracts;

namespace PersonalBrand.Application.Education;

/// <summary>
/// Education use cases.
/// </summary>
public interface IEducationService
{
    Task<IReadOnlyList<EducationEntryDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<EducationEntryDto> GetPublishedByIdAsync(Guid educationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationEntryDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<EducationEntryDto> CreateMineAsync(CreateEducationEntryRequest request, CancellationToken cancellationToken = default);

    Task<EducationEntryDto> UpdateMineAsync(Guid educationId, UpdateEducationEntryRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid educationId, CancellationToken cancellationToken = default);

    Task<EducationEntryDto> PublishMineAsync(Guid educationId, CancellationToken cancellationToken = default);

    Task<EducationEntryDto> UnpublishMineAsync(Guid educationId, CancellationToken cancellationToken = default);
}
