using Microsoft.AspNetCore.Identity;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class UserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        // Only seed essential admin users
        await SeedSuperAdminAsync(userManager);
        await SeedAdminAsync(userManager);
    }
    
    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var superAdminEmail = "superadmin@example.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                Nickname = "SuperAdmin",
                Description = "Super administrator with full system access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = true
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
    }
    
    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                Nickname = "Admin",
                Description = "System administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}