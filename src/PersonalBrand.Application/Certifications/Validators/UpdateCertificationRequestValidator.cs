using FluentValidation;

using PersonalBrand.Application.Certifications.Contracts;

namespace PersonalBrand.Application.Certifications.Validators;

public sealed class UpdateCertificationRequestValidator : AbstractValidator<UpdateCertificationRequest>
{
    public UpdateCertificationRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Issuer)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.CredentialId)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.CredentialId));

        RuleFor(request => request.CredentialUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.CredentialUrl));

        RuleFor(request => request.Description)
            .MaximumLength(2000)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.ExpiresOn)
            .NotNull()
            .When(request => !request.DoesNotExpire);

        RuleFor(request => request.ExpiresOn)
            .GreaterThanOrEqualTo(request => request.IssuedOn)
            .When(request => !request.DoesNotExpire && request.ExpiresOn.HasValue);

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}
