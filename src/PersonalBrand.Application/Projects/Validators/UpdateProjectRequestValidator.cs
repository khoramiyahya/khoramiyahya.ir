using FluentValidation;

using PersonalBrand.Application.Projects.Contracts;

namespace PersonalBrand.Application.Projects.Validators;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
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

        RuleFor(request => request.ProjectUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.ProjectUrl));

        RuleFor(request => request.RepoUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.RepoUrl));

        RuleFor(request => request.Technologies)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Technologies));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
