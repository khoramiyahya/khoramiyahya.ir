using PersonalBrand.Application.Skills.Contracts;

namespace PersonalBrand.Application.Skills;

/// <summary>
/// Skill catalog use cases.
/// </summary>
public interface ISkillService
{
    Task<IReadOnlyList<SkillDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<SkillDto> GetPublishedByIdAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<SkillDto> CreateMineAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);

    Task<SkillDto> UpdateMineAsync(Guid skillId, UpdateSkillRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<SkillDto> PublishMineAsync(Guid skillId, CancellationToken cancellationToken = default);

    Task<SkillDto> UnpublishMineAsync(Guid skillId, CancellationToken cancellationToken = default);
}
