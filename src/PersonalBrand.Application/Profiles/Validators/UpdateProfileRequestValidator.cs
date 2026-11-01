using FluentValidation;

using PersonalBrand.Application.Profiles.Contracts;

namespace PersonalBrand.Application.Profiles.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Slug)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Slug));

        RuleFor(request => request.Headline)
            .MaximumLength(300)
            .When(request => !string.IsNullOrWhiteSpace(request.Headline));

        RuleFor(request => request.Bio)
            .MaximumLength(4000)
            .When(request => !string.IsNullOrWhiteSpace(request.Bio));

        RuleFor(request => request.Location)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Location));

        RuleFor(request => request.WebsiteUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.WebsiteUrl));

        RuleFor(request => request.LinkedInUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.LinkedInUrl));

        RuleFor(request => request.GitHubUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.GitHubUrl));

        RuleFor(request => request.TwitterUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.TwitterUrl));

        RuleFor(request => request.InstagramUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.InstagramUrl));
    }
}
