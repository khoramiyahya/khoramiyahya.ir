using FluentValidation;

using PersonalBrand.Application.Newsletter.Contracts;

namespace PersonalBrand.Application.Newsletter.Validators;

public sealed class SubscribeNewsletterRequestValidator : AbstractValidator<SubscribeNewsletterRequest>
{
    public SubscribeNewsletterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Name)
            .MaximumLength(150)
            .When(request => !string.IsNullOrWhiteSpace(request.Name));
    }
}
