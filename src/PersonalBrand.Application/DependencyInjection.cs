using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using PersonalBrand.Application.Options;

namespace PersonalBrand.Application;

/// <summary>
/// Registers application-layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer)
                && !string.IsNullOrWhiteSpace(options.Audience)
                && !string.IsNullOrWhiteSpace(options.SigningKey)
                && options.AccessTokenExpirationMinutes > 0
                && options.RefreshTokenExpirationDays > 0,
                "Jwt configuration is incomplete.")
            .ValidateOnStart();

        services
            .AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName);

        return services;
    }
}
