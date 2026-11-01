using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Auth;
using PersonalBrand.Infrastructure.Auth;
using PersonalBrand.Infrastructure.Identity;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;
using PersonalBrand.Infrastructure.Services;
using PersonalBrand.Infrastructure.Storage;

namespace PersonalBrand.Infrastructure;

/// <summary>
/// Registers infrastructure services, persistence, and Identity.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<FileStorageOptions>()
            .BindConfiguration(FileStorageOptions.SectionName);

        services
            .AddOptions<BootstrapAdminOptions>()
            .BindConfiguration(BootstrapAdminOptions.SectionName);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. Configure it via appsettings or environment variables.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IAuditLogger, SerilogAuditLogger>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
