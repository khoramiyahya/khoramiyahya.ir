namespace PersonalBrand.Application.Media.Contracts;

/// <summary>
/// Request payload for updating media metadata.
/// </summary>
public sealed class UpdateMediaAssetRequest
{
    public string? Title { get; set; }

    public string? AltText { get; set; }
}
