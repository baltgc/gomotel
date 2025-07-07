using System;
using Gomotel.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Gomotel.Infrastructure.Data;

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        try
        {
            // Ensure database is created with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await context.Database.EnsureCreatedAsync(cts.Token);

            // Seed roles
            await SeedRoles(roleManager);

            // Seed default admin user
            await SeedDefaultAdmin(userManager);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Database initialization timed out after 30 seconds");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize database", ex);
        }
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "MotelAdmin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedDefaultAdmin(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@gomotel.com";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
