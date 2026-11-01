using FluentValidation;

using PersonalBrand.Application.Awards.Contracts;

namespace PersonalBrand.Application.Awards.Validators;

public sealed class CreateAwardRequestValidator : AbstractValidator<CreateAwardRequest>
{
    public CreateAwardRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Issuer)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(2000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.Url)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("URL must be a valid http or https URL.")
            .When(request => !string.IsNullOrWhiteSpace(request.Url));

        RuleFor(request => request.AwardedOn)
            .NotEmpty();

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
