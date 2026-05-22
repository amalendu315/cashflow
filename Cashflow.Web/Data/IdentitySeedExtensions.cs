using Cashflow.Web.Constants;
using Cashflow.Web.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cashflow.Web.Data;

public static class IdentitySeedExtensions
{
    public static async Task SeedIdentityDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        IConfiguration configuration =
            scope.ServiceProvider.GetRequiredService<IConfiguration>();

        ILogger logger =
            scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("IdentitySeed");

        foreach (string roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create role '{roleName}'. {FormatIdentityErrors(roleResult.Errors)}");
            }

            logger.LogInformation("Created role: {RoleName}", roleName);
        }

        string? adminEmail = configuration["SeedAdmin:Email"];
        string? adminPassword = configuration["SeedAdmin:Password"];
        string adminFullName = configuration["SeedAdmin:FullName"] ?? "System Administrator";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed admin skipped because SeedAdmin:Email or SeedAdmin:Password is missing.");

            return;
        }

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminFullName,
                CompanyId = null,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create seed admin user. {FormatIdentityErrors(createResult.Errors)}");
            }

            logger.LogInformation("Created seed admin user: {Email}", adminEmail);
        }
        else
        {
            bool userChanged = false;

            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                userChanged = true;
            }

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                userChanged = true;
            }

            if (adminUser.CompanyId is not null)
            {
                adminUser.CompanyId = null;
                userChanged = true;
            }

            if (string.IsNullOrWhiteSpace(adminUser.FullName))
            {
                adminUser.FullName = adminFullName;
                userChanged = true;
            }

            if (userChanged)
            {
                IdentityResult updateResult = await userManager.UpdateAsync(adminUser);

                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not update seed admin user. {FormatIdentityErrors(updateResult.Errors)}");
                }
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            IdentityResult addRoleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);

            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign Admin role to seed admin user. {FormatIdentityErrors(addRoleResult.Errors)}");
            }

            logger.LogInformation("Assigned Admin role to seed admin user: {Email}", adminEmail);
        }
    }

    private static string FormatIdentityErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(error => $"{error.Code}: {error.Description}"));
    }
}