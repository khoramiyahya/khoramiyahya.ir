using PersonalBrand.Application.Navigation.Contracts;

namespace PersonalBrand.Application.Navigation;

/// <summary>
/// Navigation menu use cases.
/// </summary>
public interface INavigationService
{
    Task<IReadOnlyList<NavigationItemDto>> GetPublishedAsync(
        string? location = null,
        CancellationToken cancellationToken = default);

    Task<NavigationItemDto> GetPublishedByIdAsync(Guid navigationItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NavigationItemDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<NavigationItemDto> CreateMineAsync(CreateNavigationItemRequest request, CancellationToken cancellationToken = default);

    Task<NavigationItemDto> UpdateMineAsync(
        Guid navigationItemId,
        UpdateNavigationItemRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid navigationItemId, CancellationToken cancellationToken = default);

    Task<NavigationItemDto> PublishMineAsync(Guid navigationItemId, CancellationToken cancellationToken = default);

    Task<NavigationItemDto> UnpublishMineAsync(Guid navigationItemId, CancellationToken cancellationToken = default);
}
