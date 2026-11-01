using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Auth;
using PersonalBrand.Application.Blog;
using PersonalBrand.Application.Certifications;
using PersonalBrand.Application.Contact;
using PersonalBrand.Application.Dashboard;
using PersonalBrand.Application.Education;
using PersonalBrand.Application.Experience;
using PersonalBrand.Application.Faq;
using PersonalBrand.Application.Media;
using PersonalBrand.Application.Newsletter;
using PersonalBrand.Application.Profiles;
using PersonalBrand.Application.Projects;
using PersonalBrand.Application.Services;
using PersonalBrand.Application.Settings;
using PersonalBrand.Application.Skills;
using PersonalBrand.Application.Testimonials;
using PersonalBrand.Infrastructure.Auth;
using PersonalBrand.Infrastructure.Blog;
using PersonalBrand.Infrastructure.Certifications;
using PersonalBrand.Infrastructure.Contact;
using PersonalBrand.Infrastructure.Dashboard;
using PersonalBrand.Infrastructure.Education;
using PersonalBrand.Infrastructure.Experience;
using PersonalBrand.Infrastructure.Faq;
using PersonalBrand.Infrastructure.Identity;
using PersonalBrand.Infrastructure.Media;
using PersonalBrand.Infrastructure.Newsletter;
using PersonalBrand.Infrastructure.Settings;
using PersonalBrand.Infrastructure.Options;
using PersonalBrand.Infrastructure.Persistence;
using PersonalBrand.Infrastructure.Profiles;
using PersonalBrand.Infrastructure.Projects;
using PersonalBrand.Infrastructure.Services;
using PersonalBrand.Infrastructure.ServicesCatalog;
using PersonalBrand.Infrastructure.Skills;
using PersonalBrand.Infrastructure.Storage;
using PersonalBrand.Infrastructure.Testimonials;

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
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IServiceOfferService, ServiceOfferService>();
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<ICertificationService, CertificationService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IMediaService, MediaService>();

        return services;
    }
}
