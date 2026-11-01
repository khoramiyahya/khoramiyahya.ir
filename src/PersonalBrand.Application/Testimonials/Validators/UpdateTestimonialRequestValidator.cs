using FluentValidation;

using PersonalBrand.Application.Testimonials.Contracts;

namespace PersonalBrand.Application.Testimonials.Validators;

public sealed class UpdateTestimonialRequestValidator : AbstractValidator<UpdateTestimonialRequest>
{
    public UpdateTestimonialRequestValidator()
    {
        RuleFor(request => request.AuthorName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.AuthorRole)
            .MaximumLength(150)
            .When(request => !string.IsNullOrWhiteSpace(request.AuthorRole));

        RuleFor(request => request.Company)
            .MaximumLength(150)
            .When(request => !string.IsNullOrWhiteSpace(request.Company));

        RuleFor(request => request.Quote)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(request => request.Rating)
            .InclusiveBetween(1, 5)
            .When(request => request.Rating.HasValue);

        RuleFor(request => request.AuthorImageUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.AuthorImageUrl));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
