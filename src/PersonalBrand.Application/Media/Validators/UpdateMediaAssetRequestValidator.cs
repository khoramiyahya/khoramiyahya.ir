using FluentValidation;

using PersonalBrand.Application.Media.Contracts;

namespace PersonalBrand.Application.Media.Validators;

public sealed class UpdateMediaAssetRequestValidator : AbstractValidator<UpdateMediaAssetRequest>
{
    public UpdateMediaAssetRequestValidator()
    {
        RuleFor(request => request.Title)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Title));

        RuleFor(request => request.AltText)
            .MaximumLength(300)
            .When(request => !string.IsNullOrWhiteSpace(request.AltText));
    }
}
