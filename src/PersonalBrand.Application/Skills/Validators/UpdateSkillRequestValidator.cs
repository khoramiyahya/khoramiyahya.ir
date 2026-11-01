using FluentValidation;

using PersonalBrand.Application.Skills.Contracts;

namespace PersonalBrand.Application.Skills.Validators;

public sealed class UpdateSkillRequestValidator : AbstractValidator<UpdateSkillRequest>
{
    public UpdateSkillRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Category)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Category));

        RuleFor(request => request.Description)
            .MaximumLength(1000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.ProficiencyPercent)
            .InclusiveBetween(0, 100);

        RuleFor(request => request.YearsExperience)
            .InclusiveBetween(0, 60)
            .When(request => request.YearsExperience.HasValue);

        RuleFor(request => request.IconName)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.IconName));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
