using FluentValidation;

using PersonalBrand.Application.Navigation.Contracts;

namespace PersonalBrand.Application.Navigation.Validators;

public sealed class CreateNavigationItemRequestValidator : AbstractValidator<CreateNavigationItemRequest>
{
    public CreateNavigationItemRequestValidator()
    {
        RuleFor(request => request.Label)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Url)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeRelativePathOrAbsoluteHttpUrl)
            .WithMessage("URL must be a relative path or a valid http/https URL.");

        RuleFor(request => request.Location)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.Location));

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeRelativePathOrAbsoluteHttpUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (trimmed.StartsWith('/') || trimmed.StartsWith('#'))
        {
            return true;
        }

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
