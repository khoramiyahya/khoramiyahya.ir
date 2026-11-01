using FluentValidation;

using PersonalBrand.Application.Talks.Contracts;

namespace PersonalBrand.Application.Talks.Validators;

public sealed class CreateTalkRequestValidator : AbstractValidator<CreateTalkRequest>
{
    public CreateTalkRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.EventName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Location)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Location));

        RuleFor(request => request.Description)
            .MaximumLength(4000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.Url)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("URL must be a valid http or https URL.")
            .When(request => !string.IsNullOrWhiteSpace(request.Url));

        RuleFor(request => request.SpokenOn)
            .NotEmpty();

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
