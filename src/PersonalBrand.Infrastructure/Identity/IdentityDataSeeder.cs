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

        var hasBootstrapEmail = !string.IsNullOrWhiteSpace(bootstrapOptions.Email);
        var hasBootstrapPassword = !string.IsNullOrWhiteSpace(bootstrapOptions.Password);

        if (!hasBootstrapEmail && !hasBootstrapPassword)
        {
            return;
        }

        if (!hasBootstrapEmail || !hasBootstrapPassword)
        {
            throw new InvalidOperationException(
                "Bootstrap administrator email and password must both be configured or both be empty.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var bootstrapEmail = bootstrapOptions.Email.Trim();
        var existingAdmin = await userManager.FindByEmailAsync(bootstrapEmail);
        if (existingAdmin is not null)
        {
            if (!await userManager.IsInRoleAsync(existingAdmin, AppRoles.Administrator))
            {
                throw new InvalidOperationException(
                    "The configured bootstrap administrator email belongs to an existing user without the Administrator role.");
            }

            logger.LogInformation("Bootstrap administrator {Email} already exists.", existingAdmin.Email);
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = bootstrapEmail,
            Email = bootstrapEmail,
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

        try
        {
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to assign Administrator role: {errors}");
            }
        }
        catch (Exception roleException)
        {
            var cleanupException = await TryDeleteIncompleteUserAsync(userManager, adminUser);
            if (cleanupException is not null)
            {
                throw new AggregateException(
                    "Bootstrap administrator role assignment and cleanup both failed.",
                    roleException,
                    cleanupException);
            }

            throw new InvalidOperationException(
                "Failed to assign the Administrator role. The incomplete bootstrap user was removed.",
                roleException);
        }

        logger.LogInformation("Seeded bootstrap administrator {Email}", adminUser.Email);
    }

    private static async Task<Exception?> TryDeleteIncompleteUserAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        try
        {
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return null;
            }

            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            return new InvalidOperationException($"Failed to remove incomplete bootstrap user: {errors}");
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}
