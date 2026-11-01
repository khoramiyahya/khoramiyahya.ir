using FluentValidation;

using PersonalBrand.Application.Settings.Contracts;

namespace PersonalBrand.Application.Settings.Validators;

public sealed class UpdateSiteSettingsRequestValidator : AbstractValidator<UpdateSiteSettingsRequest>
{
    public UpdateSiteSettingsRequestValidator()
    {
        RuleFor(request => request.SiteName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Tagline)
            .MaximumLength(300)
            .When(request => !string.IsNullOrWhiteSpace(request.Tagline));

        RuleFor(request => request.MetaTitle)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.MetaTitle));

        RuleFor(request => request.MetaDescription)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.MetaDescription));

        RuleFor(request => request.MetaKeywords)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.MetaKeywords));

        RuleFor(request => request.CanonicalBaseUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.CanonicalBaseUrl));

        RuleFor(request => request.OgImageUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.OgImageUrl));

        RuleFor(request => request.FaviconUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.FaviconUrl));

        RuleFor(request => request.PrimaryEmail)
            .EmailAddress()
            .MaximumLength(256)
            .When(request => !string.IsNullOrWhiteSpace(request.PrimaryEmail));

        RuleFor(request => request.PrimaryPhone)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.PrimaryPhone));

        RuleFor(request => request.Address)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Address));

        RuleFor(request => request.GoogleAnalyticsId)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.GoogleAnalyticsId));
    }
}
