using FluentValidation;

using PersonalBrand.Application.Languages.Contracts;

namespace PersonalBrand.Application.Languages.Validators;

public sealed class CreateSpokenLanguageRequestValidator : AbstractValidator<CreateSpokenLanguageRequest>
{
    public CreateSpokenLanguageRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Proficiency)
            .NotEmpty()
            .Must(BeAllowedProficiency)
            .WithMessage("Proficiency must be native, fluent, professional, intermediate, or basic.");

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAllowedProficiency(string proficiency) =>
        proficiency.Equals("native", StringComparison.OrdinalIgnoreCase)
        || proficiency.Equals("fluent", StringComparison.OrdinalIgnoreCase)
        || proficiency.Equals("professional", StringComparison.OrdinalIgnoreCase)
        || proficiency.Equals("intermediate", StringComparison.OrdinalIgnoreCase)
        || proficiency.Equals("basic", StringComparison.OrdinalIgnoreCase);
}
