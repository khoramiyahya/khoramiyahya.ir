using PersonalBrand.Application.Talks.Contracts;

namespace PersonalBrand.Application.Talks;

/// <summary>
/// Speaking engagement use cases.
/// </summary>
public interface ITalkService
{
    Task<IReadOnlyList<TalkDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<TalkDto> GetPublishedByIdAsync(Guid talkId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TalkDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<TalkDto> CreateMineAsync(CreateTalkRequest request, CancellationToken cancellationToken = default);

    Task<TalkDto> UpdateMineAsync(
        Guid talkId,
        UpdateTalkRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid talkId, CancellationToken cancellationToken = default);

    Task<TalkDto> PublishMineAsync(Guid talkId, CancellationToken cancellationToken = default);

    Task<TalkDto> UnpublishMineAsync(Guid talkId, CancellationToken cancellationToken = default);
}
