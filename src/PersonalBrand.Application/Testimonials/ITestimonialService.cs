using PersonalBrand.Application.Testimonials.Contracts;

namespace PersonalBrand.Application.Testimonials;

/// <summary>
/// Testimonial use cases.
/// </summary>
public interface ITestimonialService
{
    Task<IReadOnlyList<TestimonialDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<TestimonialDto> GetPublishedByIdAsync(Guid testimonialId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestimonialDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<TestimonialDto> CreateMineAsync(CreateTestimonialRequest request, CancellationToken cancellationToken = default);

    Task<TestimonialDto> UpdateMineAsync(Guid testimonialId, UpdateTestimonialRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid testimonialId, CancellationToken cancellationToken = default);

    Task<TestimonialDto> PublishMineAsync(Guid testimonialId, CancellationToken cancellationToken = default);

    Task<TestimonialDto> UnpublishMineAsync(Guid testimonialId, CancellationToken cancellationToken = default);
}
