using Microsoft.Extensions.Options;

using PersonalBrand.Application.Options;

namespace PersonalBrand.WebAPI.Extensions;

internal static class CorsExtensions
{
    public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        services.AddCors();
        return services;
    }

    public static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app)
    {
        var corsOptions = app.ApplicationServices.GetRequiredService<IOptions<CorsOptions>>().Value;
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        return app.UseCors(policyBuilder =>
        {
            if (corsOptions.AllowedOrigins is { Length: > 0 })
            {
                policyBuilder
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                return;
            }

            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must contain at least one origin outside Development.");
            }

            // An unrestricted fallback is permitted only for local development bootstrapping.
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
}
