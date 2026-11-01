using FluentValidation;

using PersonalBrand.Application.Common.Exceptions;

namespace PersonalBrand.Application.Common.Validation;

/// <summary>
/// Shared FluentValidation helpers for application use cases.
/// </summary>
public static class ValidationExtensions
{
    public static async Task EnsureValidAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

        throw new AppValidationException(errors);
    }
}
