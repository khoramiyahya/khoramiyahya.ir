using FluentValidation;

using PersonalBrand.Application.Newsletter.Contracts;

namespace PersonalBrand.Application.Newsletter.Validators;

public sealed class UnsubscribeNewsletterRequestValidator : AbstractValidator<UnsubscribeNewsletterRequest>
{
    public UnsubscribeNewsletterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
