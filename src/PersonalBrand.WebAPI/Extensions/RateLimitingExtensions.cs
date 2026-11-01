using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace PersonalBrand.WebAPI.Extensions;

/// <summary>
/// Rate limiting readiness using the built-in ASP.NET Core rate limiter.
/// </summary>
internal static class RateLimitingExtensions
{
    public const string GlobalPolicyName = "global";

    public const string ContactPolicyName = "contact";

    public const string NewsletterPolicyName = "newsletter";

    public static IServiceCollection AddConfiguredRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);
        var contactPermitLimit = configuration.GetValue("RateLimiting:ContactPermitLimit", 10);
        var contactWindowSeconds = configuration.GetValue("RateLimiting:ContactWindowSeconds", 60);
        var newsletterPermitLimit = configuration.GetValue("RateLimiting:NewsletterPermitLimit", 10);
        var newsletterWindowSeconds = configuration.GetValue("RateLimiting:NewsletterWindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter(GlobalPolicyName, limiterOptions =>
            {
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter(ContactPolicyName, limiterOptions =>
            {
                limiterOptions.PermitLimit = contactPermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(contactWindowSeconds);
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter(NewsletterPolicyName, limiterOptions =>
            {
                limiterOptions.PermitLimit = newsletterPermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(newsletterWindowSeconds);
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}
