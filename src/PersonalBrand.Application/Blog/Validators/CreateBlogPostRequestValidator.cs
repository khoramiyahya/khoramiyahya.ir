using FluentValidation;

using PersonalBrand.Application.Blog.Contracts;

namespace PersonalBrand.Application.Blog.Validators;

public sealed class CreateBlogPostRequestValidator : AbstractValidator<CreateBlogPostRequest>
{
    public CreateBlogPostRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Slug)
            .MaximumLength(100)
            .When(request => !string.IsNullOrWhiteSpace(request.Slug));

        RuleFor(request => request.Summary)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Summary));

        RuleFor(request => request.Body)
            .MaximumLength(50000)
            .When(request => !string.IsNullOrWhiteSpace(request.Body));

        RuleFor(request => request.Tags)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Tags));
    }
}
