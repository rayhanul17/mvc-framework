using Microsoft.AspNetCore.Identity;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
    {
        // Seed only essential roles
        string[] roleNames = 
        { 
            "SuperAdmin",     // Full system access
            "Administrator",  // Admin access
            "Manager",        // Manager with extended permissions
            "User"           // Regular user access
        };
        
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
}