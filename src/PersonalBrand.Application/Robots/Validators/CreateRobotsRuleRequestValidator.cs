using FluentValidation;

using PersonalBrand.Application.Robots.Contracts;

namespace PersonalBrand.Application.Robots.Validators;

public sealed class CreateRobotsRuleRequestValidator : AbstractValidator<CreateRobotsRuleRequest>
{
    public CreateRobotsRuleRequestValidator()
    {
        RuleFor(request => request.UserAgent)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.UserAgent));

        RuleFor(request => request.Directive)
            .NotEmpty()
            .Must(directive => directive.Equals("Allow", StringComparison.OrdinalIgnoreCase)
                || directive.Equals("Disallow", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Directive must be Allow or Disallow.");

        RuleFor(request => request.Path)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
