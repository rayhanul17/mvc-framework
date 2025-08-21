using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            logger.LogInformation("Starting database seeding...");

            // Seed only essential data
            
            // 1. Seed Roles (no dependencies)
            logger.LogInformation("Seeding roles...");
            await RoleSeeder.SeedAsync(roleManager);
            
            // 2. Seed Users (depends on Roles) - Only admin users
            logger.LogInformation("Seeding users...");
            await UserSeeder.SeedAsync(userManager);
            
            // 3. Seed Menus (depends on Roles)
            logger.LogInformation("Seeding menus...");
            await MenuSeeder.SeedAsync(context, roleManager);
            
            // 4. Seed Site Settings (essential for app configuration)
            logger.LogInformation("Seeding site settings...");
            await SiteSettingSeeder.SeedAsync(context);
            
            // 5. Seed Blog Categories
            logger.LogInformation("Seeding blog categories...");
            await BlogCategorySeeder.SeedAsync(context);
            
            // 6. Seed Blog Posts (depends on Categories and Users)
            logger.LogInformation("Seeding blog posts...");
            await BlogPostSeeder.SeedAsync(context, userManager);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}

// Extension method for IServiceProvider
public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await DatabaseSeeder.SeedAsync(serviceProvider);
    }
}