using PersonalBrand.Application.Media.Contracts;

namespace PersonalBrand.Application.Media;

/// <summary>
/// Media library use cases.
/// </summary>
public interface IMediaService
{
    Task<IReadOnlyList<MediaAssetDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<MediaAssetDto> GetMineByIdAsync(Guid mediaId, CancellationToken cancellationToken = default);

    Task<MediaAssetDto> UploadMineAsync(
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        string? title,
        string? altText,
        CancellationToken cancellationToken = default);

    Task<MediaAssetDto> UpdateMineAsync(
        Guid mediaId,
        UpdateMediaAssetRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid mediaId, CancellationToken cancellationToken = default);
}
