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
            // Home - Public menu
            new Menu
            {
                Name = "Home",
                DisplayName = "Home",
                Controller = "Home",
                Action = "Index",
                Url = "/",
                ActiveMenuUrl = "/",
                Icon = "fas fa-home",
                Order = 0,
                IsActive = true,
                AllowAnonymous = true,
                RequireAuthentication = false
            },
            
            // Dashboard - Requires authentication
            new Menu
            {
                Name = "Dashboard",
                DisplayName = "Dashboard",
                Controller = "Home",
                Action = "Dashboard",
                Url = "/Home/Dashboard",
                ActiveMenuUrl = "/Home/Dashboard",
                Icon = "fas fa-tachometer-alt",
                Order = 1,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = true
            },
            
            // Blog Management (for Admin)
            new Menu
            {
                Name = "Blog",
                DisplayName = "Blog Management",
                Icon = "fas fa-blog",
                Order = 2,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = false,
                ActiveMenuUrl = "/Blog"
            },
            
            // User Management (for SuperAdmin only)
            new Menu
            {
                Name = "UserManagement",
                DisplayName = "User Management",
                Icon = "fas fa-users",
                Order = 3,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = false,
                ActiveMenuUrl = "/User"
            },
            
            // Settings (for SuperAdmin only)
            new Menu
            {
                Name = "Settings",
                DisplayName = "Settings",
                Icon = "fas fa-cog",
                Order = 4,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = false,
                ActiveMenuUrl = "/SiteSetting"
            },
            
            // Customer Support (for Support roles)
            new Menu
            {
                Name = "CustomerSupport",
                DisplayName = "Customer Support",
                Area = "CustomerSupport",
                Icon = "fas fa-headset",
                Order = 5,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = false,
                ActiveMenuUrl = "/CustomerSupport"
            },
            
            // Audit Logs (for SuperAdmin only)
            new Menu
            {
                Name = "AuditLogs",
                DisplayName = "Audit Logs",
                Controller = "Log",
                Action = "Index",
                Url = "/Log",
                ActiveMenuUrl = "/Log",
                Icon = "fas fa-history",
                Order = 6,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = false
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
                    Url = "/BlogPost",
                    ActiveMenuUrl = "/BlogPost",
                    Icon = "fas fa-file-alt",
                    ParentId = blogMenu.Id,
                    Order = 1,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = false
                },
                new Menu
                {
                    Name = "BlogCategories",
                    DisplayName = "Categories",
                    Controller = "BlogCategory",
                    Action = "Index",
                    Url = "/BlogCategory",
                    ActiveMenuUrl = "/BlogCategory",
                    Icon = "fas fa-folder",
                    ParentId = blogMenu.Id,
                    Order = 2,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = false
                },
                new Menu
                {
                    Name = "BlogTags",
                    DisplayName = "Tags",
                    Controller = "BlogTag",
                    Action = "Index",
                    Url = "/BlogTag",
                    ActiveMenuUrl = "/BlogTag",
                    Icon = "fas fa-tags",
                    ParentId = blogMenu.Id,
                    Order = 3,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = false
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
                },
                new Menu
                {
                    Name = "Permissions",
                    DisplayName = "Permission Management",
                    Controller = "Permission",
                    Action = "Index",
                    Icon = "fas fa-shield-alt",
                    ParentId = userManagementMenu.Id,
                    Order = 4,
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
                    Name = "SupportHome",
                    DisplayName = "Overview",
                    Area = "CustomerSupport",
                    Controller = "Home",
                    Action = "Index",
                    Url = "/CustomerSupport/Home",
                    ActiveMenuUrl = "/CustomerSupport/Home",
                    Icon = "fas fa-home",
                    ParentId = supportMenu.Id,
                    Order = 1,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "SupportDashboard",
                    DisplayName = "Support Dashboard",
                    Area = "CustomerSupport",
                    Controller = "Dashboard",
                    Action = "Index",
                    Url = "/CustomerSupport/Dashboard",
                    ActiveMenuUrl = "/CustomerSupport/Dashboard",
                    Icon = "fas fa-tachometer-alt",
                    ParentId = supportMenu.Id,
                    Order = 2,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "TicketManagement",
                    DisplayName = "Tickets",
                    Area = "CustomerSupport",
                    Controller = "Ticket",
                    Action = "Index",
                    Url = "/CustomerSupport/Ticket",
                    ActiveMenuUrl = "/CustomerSupport/Ticket",
                    Icon = "fas fa-ticket-alt",
                    ParentId = supportMenu.Id,
                    Order = 3,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "ManageTickets",
                    DisplayName = "Manage Tickets",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Index",
                    Url = "/CustomerSupport/ManageTicket",
                    ActiveMenuUrl = "/CustomerSupport/ManageTicket",
                    Icon = "fas fa-tasks",
                    ParentId = supportMenu.Id,
                    Order = 4,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "SupportTickets",
                    DisplayName = "My Support Tickets",
                    Area = "CustomerSupport",
                    Controller = "SupportTicket",
                    Action = "MyTickets",
                    Url = "/CustomerSupport/SupportTicket/MyTickets",
                    ActiveMenuUrl = "/CustomerSupport/SupportTicket",
                    Icon = "fas fa-user-circle",
                    ParentId = supportMenu.Id,
                    Order = 5,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "TicketDashboard",
                    DisplayName = "Analytics",
                    Area = "CustomerSupport",
                    Controller = "Ticket",
                    Action = "Dashboard",
                    Url = "/CustomerSupport/Ticket/Dashboard",
                    ActiveMenuUrl = "/CustomerSupport/Ticket/Dashboard",
                    Icon = "fas fa-chart-pie",
                    ParentId = supportMenu.Id,
                    Order = 6,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                },
                new Menu
                {
                    Name = "SupportConfiguration",
                    DisplayName = "Configuration",
                    Area = "CustomerSupport", 
                    Controller = "Configuration",
                    Action = "Index",
                    Url = "/CustomerSupport/Configuration",
                    ActiveMenuUrl = "/CustomerSupport/Configuration",
                    Icon = "fas fa-cog",
                    ParentId = supportMenu.Id,
                    Order = 7,
                    IsActive = true,
                    AllowAnonymous = false,
                    RequireAuthentication = true
                }
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