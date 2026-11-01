using PersonalBrand.Application.Faq.Contracts;

namespace PersonalBrand.Application.Faq;

/// <summary>
/// FAQ use cases.
/// </summary>
public interface IFaqService
{
    Task<IReadOnlyList<FaqItemDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<FaqItemDto> GetPublishedByIdAsync(Guid faqId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaqItemDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<FaqItemDto> CreateMineAsync(CreateFaqItemRequest request, CancellationToken cancellationToken = default);

    Task<FaqItemDto> UpdateMineAsync(Guid faqId, UpdateFaqItemRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid faqId, CancellationToken cancellationToken = default);

    Task<FaqItemDto> PublishMineAsync(Guid faqId, CancellationToken cancellationToken = default);

    Task<FaqItemDto> UnpublishMineAsync(Guid faqId, CancellationToken cancellationToken = default);
}
