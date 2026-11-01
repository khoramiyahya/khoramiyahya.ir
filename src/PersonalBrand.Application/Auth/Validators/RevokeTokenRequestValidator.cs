using FluentValidation;

using PersonalBrand.Application.Auth.Contracts;

namespace PersonalBrand.Application.Auth.Validators;

public sealed class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .MaximumLength(512);
    }
}
