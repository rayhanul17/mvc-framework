using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class MenuSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        if (await context.Menus.AnyAsync())
            return;

        var menus = new List<Menu>
        {
            // Dashboard
            new Menu
            {
                Name = "Dashboard",
                DisplayName = "Dashboard",
                Controller = "Home",
                Action = "Dashboard",
                Icon = "fas fa-tachometer-alt",
                Order = 1,
                IsActive = true
            },
            
            // Blog Management (for Admin)
            new Menu
            {
                Name = "Blog",
                DisplayName = "Blog Management",
                Icon = "fas fa-blog",
                Order = 2,
                IsActive = true
            },
            
            // User Management (for SuperAdmin only)
            new Menu
            {
                Name = "UserManagement",
                DisplayName = "User Management",
                Icon = "fas fa-users",
                Order = 3,
                IsActive = true
            },
            
            // Settings (for SuperAdmin only)
            new Menu
            {
                Name = "Settings",
                DisplayName = "Settings",
                Icon = "fas fa-cog",
                Order = 4,
                IsActive = true
            },
            
            // Customer Support (for SuperAdmin only)
            new Menu
            {
                Name = "CustomerSupport",
                DisplayName = "Customer Support",
                Area = "CustomerSupport",
                Icon = "fas fa-headset",
                Order = 5,
                IsActive = true
            },
            
            // Audit Logs (for SuperAdmin only)
            new Menu
            {
                Name = "AuditLogs",
                DisplayName = "Audit Logs",
                Controller = "Log",
                Action = "Index",
                Icon = "fas fa-history",
                Order = 6,
                IsActive = true
            }
        };

        await context.Menus.AddRangeAsync(menus);
        await context.SaveChangesAsync();

        // Add Blog sub-menus
        await AddBlogSubMenusAsync(context);
        
        // Add User Management sub-menus
        await AddUserManagementSubMenusAsync(context);
        
        // Add Settings sub-menus
        await AddSettingsSubMenusAsync(context);
        
        // Add Customer Support sub-menus
        await AddCustomerSupportSubMenusAsync(context);
        
        // Assign menus to roles
        await AssignMenuRolesAsync(context, roleManager);
    }
    
    private static async Task AddBlogSubMenusAsync(ApplicationDbContext context)
    {
        var blogMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Blog");
        if (blogMenu != null)
        {
            var blogSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "BlogPosts",
                    DisplayName = "Posts",
                    Controller = "BlogPost",
                    Action = "Index",
                    Icon = "fas fa-file-alt",
                    ParentId = blogMenu.Id,
                    Order = 1,
                    IsActive = true
                },
                new Menu
                {
                    Name = "BlogCategories",
                    DisplayName = "Categories",
                    Controller = "BlogCategory",
                    Action = "Index",
                    Icon = "fas fa-folder",
                    ParentId = blogMenu.Id,
                    Order = 2,
                    IsActive = true
                },
                new Menu
                {
                    Name = "BlogTags",
                    DisplayName = "Tags",
                    Controller = "BlogTag",
                    Action = "Index",
                    Icon = "fas fa-tags",
                    ParentId = blogMenu.Id,
                    Order = 3,
                    IsActive = true
                },
                new Menu
                {
                    Name = "BlogImport",
                    DisplayName = "Import/Export",
                    Controller = "BlogImport",
                    Action = "Index",
                    Icon = "fas fa-file-import",
                    ParentId = blogMenu.Id,
                    Order = 4,
                    IsActive = true
                }
            };

            await context.Menus.AddRangeAsync(blogSubMenus);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task AddUserManagementSubMenusAsync(ApplicationDbContext context)
    {
        var userManagementMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "UserManagement");
        if (userManagementMenu != null)
        {
            var userSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "Users",
                    DisplayName = "Users",
                    Controller = "User",
                    Action = "Index",
                    Icon = "fas fa-user",
                    ParentId = userManagementMenu.Id,
                    Order = 1,
                    IsActive = true
                },
                new Menu
                {
                    Name = "Roles",
                    DisplayName = "Roles",
                    Controller = "Role",
                    Action = "Index",
                    Icon = "fas fa-user-tag",
                    ParentId = userManagementMenu.Id,
                    Order = 2,
                    IsActive = true
                },
                new Menu
                {
                    Name = "Menus",
                    DisplayName = "Menu Management",
                    Controller = "Menu",
                    Action = "Index",
                    Icon = "fas fa-bars",
                    ParentId = userManagementMenu.Id,
                    Order = 3,
                    IsActive = true
                }
            };

            await context.Menus.AddRangeAsync(userSubMenus);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task AddSettingsSubMenusAsync(ApplicationDbContext context)
    {
        var settingsMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Settings");
        if (settingsMenu != null)
        {
            var settingsSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "SiteSettings",
                    DisplayName = "Site Settings",
                    Controller = "SiteSetting",
                    Action = "Index",
                    Icon = "fas fa-globe",
                    ParentId = settingsMenu.Id,
                    Order = 1,
                    IsActive = true
                },
                new Menu
                {
                    Name = "FileDocuments",
                    DisplayName = "File Management",
                    Controller = "FileDocument",
                    Action = "Index",
                    Icon = "fas fa-file",
                    ParentId = settingsMenu.Id,
                    Order = 2,
                    IsActive = true
                }
            };

            await context.Menus.AddRangeAsync(settingsSubMenus);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task AddCustomerSupportSubMenusAsync(ApplicationDbContext context)
    {
        var supportMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "CustomerSupport");
        if (supportMenu != null)
        {
            var supportSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "SupportDashboard",
                    DisplayName = "Dashboard",
                    Area = "CustomerSupport",
                    Controller = "Dashboard",
                    Action = "Index",
                    Icon = "fas fa-chart-line",
                    ParentId = supportMenu.Id,
                    Order = 1,
                    IsActive = true
                },
                new Menu
                {
                    Name = "SupportTickets",
                    DisplayName = "Tickets",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Index",
                    Icon = "fas fa-ticket-alt",
                    ParentId = supportMenu.Id,
                    Order = 2,
                    IsActive = true
                },
            };

            await context.Menus.AddRangeAsync(supportSubMenus);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task AssignMenuRolesAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        var adminRole = await roleManager.FindByNameAsync("Administrator");

        if (superAdminRole == null || adminRole == null)
            return;

        // Assign all menus to SuperAdmin (already handled by IsSuperAdmin flag)
        // No need to explicitly assign menus to SuperAdmin

        // Assign Blog menus to Administrator role
        var blogMenus = await context.Menus
            .Where(m => m.Name.StartsWith("Blog") || m.Parent!.Name.StartsWith("Blog"))
            .ToListAsync();

        foreach (var menu in blogMenus)
        {
            var roleMenu = new RoleMenu
            {
                MenuId = menu.Id,
                RoleId = adminRole.Id
            };
            context.RoleMenus.Add(roleMenu);
        }

        // Assign Dashboard to Administrator role
        var dashboardMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Dashboard");
        if (dashboardMenu != null)
        {
            var roleMenu = new RoleMenu
            {
                MenuId = dashboardMenu.Id,
                RoleId = adminRole.Id
            };
            context.RoleMenus.Add(roleMenu);
        }

        await context.SaveChangesAsync();
    }
}