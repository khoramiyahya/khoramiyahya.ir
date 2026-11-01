using PersonalBrand.Application.Experience.Contracts;

namespace PersonalBrand.Application.Experience;

/// <summary>
/// Work experience use cases.
/// </summary>
public interface IExperienceService
{
    Task<IReadOnlyList<WorkExperienceDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> GetPublishedByIdAsync(Guid experienceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkExperienceDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> CreateMineAsync(CreateWorkExperienceRequest request, CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> UpdateMineAsync(Guid experienceId, UpdateWorkExperienceRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid experienceId, CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> PublishMineAsync(Guid experienceId, CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> UnpublishMineAsync(Guid experienceId, CancellationToken cancellationToken = default);
}
