using FluentValidation;

using PersonalBrand.Application.Contact.Contracts;

namespace PersonalBrand.Application.Contact.Validators;

public sealed class SubmitContactRequestValidator : AbstractValidator<SubmitContactRequest>
{
    public SubmitContactRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Phone)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.Phone));

        RuleFor(request => request.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Body)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
