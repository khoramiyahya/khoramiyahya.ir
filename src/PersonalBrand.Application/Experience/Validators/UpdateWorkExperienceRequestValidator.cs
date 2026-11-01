using FluentValidation;

using PersonalBrand.Application.Experience.Contracts;

namespace PersonalBrand.Application.Experience.Validators;

public sealed class UpdateWorkExperienceRequestValidator : AbstractValidator<UpdateWorkExperienceRequest>
{
    public UpdateWorkExperienceRequestValidator()
    {
        RuleFor(request => request.Company)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Location)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Location));

        RuleFor(request => request.Description)
            .MaximumLength(4000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.CompanyUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.CompanyUrl));

        RuleFor(request => request.EndedOn)
            .GreaterThanOrEqualTo(request => request.StartedOn)
            .When(request => !request.IsCurrent && request.EndedOn.HasValue);

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
