using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Infrastructure.Data;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting database seeding process...");
        
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("DynamicRoleMenuSystem.Web/appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Create service collection
        var services = new ServiceCollection();
        
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
            ));
        
        // Add Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        // Build service provider
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                
                Console.WriteLine("Running database initialization...");
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
                
                // Specifically update superadmin user
                Console.WriteLine("Updating superadmin user with IsSuperAdmin flag...");
                var superAdminUser = await userManager.FindByEmailAsync("superadmin@example.com");
                
                if (superAdminUser != null)
                {
                    if (!superAdminUser.IsSuperAdmin)
                    {
                        superAdminUser.IsSuperAdmin = true;
                        var result = await userManager.UpdateAsync(superAdminUser);
                        
                        if (result.Succeeded)
                        {
                            Console.WriteLine("✓ SuperAdmin user updated successfully with IsSuperAdmin = true");
                        }
                        else
                        {
                            Console.WriteLine("✗ Failed to update SuperAdmin user:");
                            foreach (var error in result.Errors)
                            {
                                Console.WriteLine($"  - {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("✓ SuperAdmin user already has IsSuperAdmin = true");
                    }
                    
                    // Display user info
                    Console.WriteLine($"\nSuperAdmin User Details:");
                    Console.WriteLine($"  Email: {superAdminUser.Email}");
                    Console.WriteLine($"  Full Name: {superAdminUser.FullName}");
                    Console.WriteLine($"  IsSuperAdmin: {superAdminUser.IsSuperAdmin}");
                    Console.WriteLine($"  IsActive: {superAdminUser.IsActive}");
                }
                else
                {
                    Console.WriteLine("✗ SuperAdmin user not found. Creating new superadmin user...");
                    
                    var newSuperAdmin = new ApplicationUser
                    {
                        UserName = "superadmin@example.com",
                        Email = "superadmin@example.com",
                        FullName = "Super Administrator",
                        Description = "Super administrator with full system access",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsSuperAdmin = true
                    };
                    
                    var createResult = await userManager.CreateAsync(newSuperAdmin, "SuperAdmin@123");
                    
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newSuperAdmin, "SuperAdmin");
                        Console.WriteLine("✓ New SuperAdmin user created successfully with IsSuperAdmin = true");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to create SuperAdmin user:");
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine($"  - {error.Description}");
                        }
                    }
                }
                
                // Run comprehensive menu seeding
                Console.WriteLine("\nSeeding all menus for SuperAdmin role...");
                await SuperAdminMenuSeeder.SeedAllMenusForSuperAdminAsync(context, roleManager);
                Console.WriteLine("✓ Menu seeding completed successfully");
                
                Console.WriteLine("\n✅ Database seeding process completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error during database seeding: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}