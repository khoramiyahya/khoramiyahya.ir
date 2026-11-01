using FluentValidation;

using PersonalBrand.Application.Pages.Contracts;

namespace PersonalBrand.Application.Pages.Validators;

public sealed class CreateContentPageRequestValidator : AbstractValidator<CreateContentPageRequest>
{
    public CreateContentPageRequestValidator()
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

        RuleFor(request => request.Body)
            .MaximumLength(50000)
            .When(request => !string.IsNullOrWhiteSpace(request.Body));

        RuleFor(request => request.MetaTitle)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.MetaTitle));

        RuleFor(request => request.MetaDescription)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.MetaDescription));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
