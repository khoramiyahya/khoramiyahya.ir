using FluentValidation;

using PersonalBrand.Application.Search.Contracts;

namespace PersonalBrand.Application.Search.Validators;

public sealed class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "project",
        "blog",
        "service",
        "page",
        "faq",
        "skill",
        "certification",
        "experience",
        "education",
        "award",
        "talk",
        "publication"
    };

    public SearchRequestValidator()
    {
        RuleFor(request => request.Query)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(request => request.Limit)
            .InclusiveBetween(1, 50);

        RuleFor(request => request.ContentType)
            .Must(type => type is null || AllowedContentTypes.Contains(type.Trim()))
            .WithMessage("Content type filter is invalid.")
            .When(request => !string.IsNullOrWhiteSpace(request.ContentType));
    }
}
