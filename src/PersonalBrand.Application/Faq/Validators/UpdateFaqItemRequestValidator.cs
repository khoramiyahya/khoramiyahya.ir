using FluentValidation;

using PersonalBrand.Application.Faq.Contracts;

namespace PersonalBrand.Application.Faq.Validators;

public sealed class UpdateFaqItemRequestValidator : AbstractValidator<UpdateFaqItemRequest>
{
    public UpdateFaqItemRequestValidator()
    {
        RuleFor(request => request.Question)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.Answer)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(request => request.Category)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Category));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
