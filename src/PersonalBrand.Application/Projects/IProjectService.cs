using PersonalBrand.Application.Projects.Contracts;

namespace PersonalBrand.Application.Projects;

/// <summary>
/// Portfolio project use cases.
/// </summary>
public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<ProjectDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateMineAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDto> UpdateMineAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectDto> PublishMineAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectDto> UnpublishMineAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectDto> UploadCoverAsync(
        Guid projectId,
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> RemoveCoverAsync(Guid projectId, CancellationToken cancellationToken = default);
}
