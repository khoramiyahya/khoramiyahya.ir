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

            // Fallback for local bootstrapping only. Production must configure Cors:AllowedOrigins.
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
}
