using FluentValidation;

using PersonalBrand.Application.Publications.Contracts;

namespace PersonalBrand.Application.Publications.Validators;

public sealed class UpdatePublicationRequestValidator : AbstractValidator<UpdatePublicationRequest>
{
    public UpdatePublicationRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Publisher)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.PublicationType)
            .NotEmpty()
            .Must(BeAllowedType)
            .WithMessage("Publication type must be article, book, paper, or podcast.");

        RuleFor(request => request.Description)
            .MaximumLength(4000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.Url)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("URL must be a valid http or https URL.")
            .When(request => !string.IsNullOrWhiteSpace(request.Url));

        RuleFor(request => request.PublishedOn)
            .NotEmpty();

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeAllowedType(string type) =>
        type.Equals("article", StringComparison.OrdinalIgnoreCase)
        || type.Equals("book", StringComparison.OrdinalIgnoreCase)
        || type.Equals("paper", StringComparison.OrdinalIgnoreCase)
        || type.Equals("podcast", StringComparison.OrdinalIgnoreCase);

    private static bool BeAbsoluteHttpUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
