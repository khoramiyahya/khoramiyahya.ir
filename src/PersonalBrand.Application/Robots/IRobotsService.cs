using PersonalBrand.Application.Robots.Contracts;

namespace PersonalBrand.Application.Robots;

/// <summary>
/// Robots.txt rule use cases.
/// </summary>
public interface IRobotsService
{
    Task<string> GetPublicTextAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RobotsRuleDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<RobotsRuleDto> CreateMineAsync(CreateRobotsRuleRequest request, CancellationToken cancellationToken = default);

    Task<RobotsRuleDto> UpdateMineAsync(
        Guid robotsRuleId,
        UpdateRobotsRuleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid robotsRuleId, CancellationToken cancellationToken = default);

    Task<RobotsRuleDto> PublishMineAsync(Guid robotsRuleId, CancellationToken cancellationToken = default);

    Task<RobotsRuleDto> UnpublishMineAsync(Guid robotsRuleId, CancellationToken cancellationToken = default);
}
