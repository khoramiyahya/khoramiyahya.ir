using FluentValidation;

using PersonalBrand.Application.Auth.Contracts;

namespace PersonalBrand.Application.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(request => request.ConfirmPassword)
            .Equal(request => request.Password)
            .WithMessage("Password confirmation does not match.");

        RuleFor(request => request.DisplayName)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.DisplayName));
    }
}
