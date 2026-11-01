using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Authorization;
using PersonalBrand.Infrastructure.Options;

namespace PersonalBrand.Infrastructure.Identity;

/// <summary>
/// Seeds built-in roles and an optional bootstrap administrator.
/// </summary>
public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var bootstrapOptions = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentityDataSeeder));

        foreach (var roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                Description = $"{roleName} role"
            });

            if (!createRoleResult.Succeeded)
            {
                var errors = string.Join("; ", createRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
            }

            logger.LogInformation("Seeded role {RoleName}", roleName);
        }

        if (string.IsNullOrWhiteSpace(bootstrapOptions.Email)
            || string.IsNullOrWhiteSpace(bootstrapOptions.Password))
        {
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(bootstrapOptions.Email.Trim());
        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = bootstrapOptions.Email.Trim(),
            Email = bootstrapOptions.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(bootstrapOptions.DisplayName)
                ? "Administrator"
                : bootstrapOptions.DisplayName.Trim(),
            EmailConfirmed = true,
            CreatedAtUtc = dateTimeProvider.UtcNow,
            IsActive = true
        };

        var createUserResult = await userManager.CreateAsync(adminUser, bootstrapOptions.Password);
        if (!createUserResult.Succeeded)
        {
            var errors = string.Join("; ", createUserResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to seed bootstrap administrator: {errors}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join("; ", addRoleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign Administrator role: {errors}");
        }

        logger.LogInformation("Seeded bootstrap administrator {Email}", adminUser.Email);
    }
}
