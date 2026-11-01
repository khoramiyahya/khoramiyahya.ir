using FluentValidation;

using PersonalBrand.Application.SocialLinks.Contracts;

namespace PersonalBrand.Application.SocialLinks.Validators;

public sealed class CreateSocialLinkRequestValidator : AbstractValidator<CreateSocialLinkRequest>
{
    public CreateSocialLinkRequestValidator()
    {
        RuleFor(request => request.Platform)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Label)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Url)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("URL must be a valid http or https URL.");

        RuleFor(request => request.Icon)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Icon));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAbsoluteHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
