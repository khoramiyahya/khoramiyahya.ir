using FluentValidation;

using PersonalBrand.Application.Redirects.Contracts;

namespace PersonalBrand.Application.Redirects.Validators;

public sealed class CreateSeoRedirectRequestValidator : AbstractValidator<CreateSeoRedirectRequest>
{
    public CreateSeoRedirectRequestValidator()
    {
        RuleFor(request => request.FromPath)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.ToUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeRelativePathOrAbsoluteHttpUrl)
            .WithMessage("Destination URL must be a relative path or a valid http/https URL.");

        RuleFor(request => request.StatusCode)
            .Must(code => code is 301 or 302 or 307 or 308)
            .WithMessage("Status code must be 301, 302, 307, or 308.");

        RuleFor(request => request.Notes)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Notes));
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
