using FluentValidation;

using PersonalBrand.Application.Partners.Contracts;

namespace PersonalBrand.Application.Partners.Validators;

public sealed class UpdatePartnerRequestValidator : AbstractValidator<UpdatePartnerRequest>
{
    public UpdatePartnerRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.WebsiteUrl)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("Website URL must be a valid http or https URL.")
            .When(request => !string.IsNullOrWhiteSpace(request.WebsiteUrl));

        RuleFor(request => request.LogoUrl)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("Logo URL must be a valid http or https URL.")
            .When(request => !string.IsNullOrWhiteSpace(request.LogoUrl));

        RuleFor(request => request.Summary)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Summary));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
