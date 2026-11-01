using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace PersonalBrand.WebAPI.Extensions;

/// <summary>
/// Rate limiting readiness using the built-in ASP.NET Core rate limiter.
/// </summary>
internal static class RateLimitingExtensions
{
    public const string GlobalPolicyName = "global";

    public static IServiceCollection AddConfiguredRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter(GlobalPolicyName, limiterOptions =>
            {
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}
