using FluentValidation;

using PersonalBrand.Application.Services.Contracts;

namespace PersonalBrand.Application.Services.Validators;

public sealed class UpdateServiceOfferRequestValidator : AbstractValidator<UpdateServiceOfferRequest>
{
    public UpdateServiceOfferRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Slug)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Slug));

        RuleFor(request => request.Summary)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Summary));

        RuleFor(request => request.Description)
            .MaximumLength(8000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.PriceLabel)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.PriceLabel));

        RuleFor(request => request.IconName)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.IconName));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
