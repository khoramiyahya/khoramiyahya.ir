using FluentValidation;

using PersonalBrand.Application.Education.Contracts;

namespace PersonalBrand.Application.Education.Validators;

public sealed class UpdateEducationEntryRequestValidator : AbstractValidator<UpdateEducationEntryRequest>
{
    public UpdateEducationEntryRequestValidator()
    {
        RuleFor(request => request.Institution)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Degree)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.FieldOfStudy)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.FieldOfStudy));

        RuleFor(request => request.Location)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Location));

        RuleFor(request => request.Description)
            .MaximumLength(4000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.InstitutionUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.InstitutionUrl));

        RuleFor(request => request.EndedOn)
            .GreaterThanOrEqualTo(request => request.StartedOn)
            .When(request => !request.IsCurrent && request.EndedOn.HasValue);

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
