using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Nexora.Web.Data;

public static class EnsureSiteSettingsMenu
{
    public static async Task EnsureMenuExistsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Check if Site Settings menu exists
        var siteSettingsMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Site Settings");

        if (siteSettingsMenu == null)
        {
            // Find the Administration parent menu
            var adminMenu = await context.Menus
                .FirstOrDefaultAsync(m => m.Name == "Administration");

            if (adminMenu != null)
            {
                // Create Site Settings menu
                siteSettingsMenu = new Menu
                {
                    Name = "Site Settings",
                    DisplayName = "Site Settings",
                    Controller = "SiteSetting",
                    Action = "Index",
                    Icon = "fas fa-cog",
                    ParentId = adminMenu.Id,
                    Order = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Menus.AddAsync(siteSettingsMenu);
                await context.SaveChangesAsync();

                // Assign to SuperAdmin role
                var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
                if (superAdminRole != null)
                {
                    var existingRoleMenu = await context.RoleMenus
                        .FirstOrDefaultAsync(rm => rm.RoleId == superAdminRole.Id && rm.MenuId == siteSettingsMenu.Id);

                    if (existingRoleMenu == null)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = superAdminRole.Id,
                            MenuId = siteSettingsMenu.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = true,
                            CanDelete = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }

                // Also assign to Administrator role with limited permissions
                var adminRole = await roleManager.FindByNameAsync("Administrator");
                if (adminRole != null)
                {
                    var existingAdminRoleMenu = await context.RoleMenus
                        .FirstOrDefaultAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == siteSettingsMenu.Id);

                    if (existingAdminRoleMenu == null)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = adminRole.Id,
                            MenuId = siteSettingsMenu.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = true,
                            CanDelete = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Site Settings menu created and assigned to roles successfully!");
            }
            else
            {
                Console.WriteLine("Administration parent menu not found!");
            }
        }
        else
        {
            // Menu exists, ensure it's assigned to SuperAdmin
            var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole != null)
            {
                var roleMenuExists = await context.RoleMenus
                    .AnyAsync(rm => rm.RoleId == superAdminRole.Id && rm.MenuId == siteSettingsMenu.Id);

                if (!roleMenuExists)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = superAdminRole.Id,
                        MenuId = siteSettingsMenu.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.RoleMenus.AddAsync(roleMenu);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Site Settings menu assigned to SuperAdmin role!");
                }
                else
                {
                    Console.WriteLine("Site Settings menu already exists and is assigned to SuperAdmin!");
                }
            }
        }
    }
}