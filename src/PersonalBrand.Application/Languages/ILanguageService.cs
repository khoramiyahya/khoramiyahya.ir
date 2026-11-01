using PersonalBrand.Application.Languages.Contracts;

namespace PersonalBrand.Application.Languages;

/// <summary>
/// Spoken language use cases.
/// </summary>
public interface ILanguageService
{
    Task<IReadOnlyList<SpokenLanguageDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<SpokenLanguageDto> GetPublishedByIdAsync(Guid languageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpokenLanguageDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<SpokenLanguageDto> CreateMineAsync(CreateSpokenLanguageRequest request, CancellationToken cancellationToken = default);

    Task<SpokenLanguageDto> UpdateMineAsync(
        Guid languageId,
        UpdateSpokenLanguageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid languageId, CancellationToken cancellationToken = default);

    Task<SpokenLanguageDto> PublishMineAsync(Guid languageId, CancellationToken cancellationToken = default);

    Task<SpokenLanguageDto> UnpublishMineAsync(Guid languageId, CancellationToken cancellationToken = default);
}
