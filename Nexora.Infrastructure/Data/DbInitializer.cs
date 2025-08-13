using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Apply migrations
        await context.Database.MigrateAsync();

        // Seed roles (only essential roles)
        await SeedRolesAsync(roleManager);

        // Seed users
        await SeedUsersAsync(userManager);

        // Seed menus
        await SeedMenusAsync(context, roleManager);
        
        // Seed blog categories
        await SeedBlogCategoriesAsync(context);
        
        // Seed site settings
        await SeedSiteSettingsAsync(context);
        
        // Seed Customer Service role mappings
        await SeedCustomerServiceRoleMappingsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        // Seed necessary roles including Customer Service roles
        string[] roleNames = { 
            "SuperAdmin", 
            "Administrator", 
            "User",
            "CustomerSupportAdmin",
            "CustomerSupportManager", 
            "CustomerSupportAgent",
            "CustomerSupportCustomer"
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

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed SuperAdmin user with IsSuperAdmin = true
        var superAdminEmail = "superadmin@example.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                Description = "Super administrator with full system access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = true  // This flag gives full access
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            
            if (result.Succeeded)
            {
                // No need to add to SuperAdmin role since IsSuperAdmin flag handles everything
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
        else
        {
            // Update existing superadmin user to have IsSuperAdmin flag
            if (!superAdminUser.IsSuperAdmin)
            {
                superAdminUser.IsSuperAdmin = true;
                await userManager.UpdateAsync(superAdminUser);
            }
        }

        // Seed Admin user (only has access to Blog)
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                Description = "Administrator with Blog management access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false  // Regular admin, not super admin
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }

        // Seed Test Users (testuser1 to testuser5) without any roles
        for (int i = 1; i <= 5; i++)
        {
            var testEmail = $"testuser{i}@example.com";
            var testUser = await userManager.FindByEmailAsync(testEmail);

            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    FullName = $"Test User {i}",
                    Description = $"Test user account {i} for testing purposes",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsSuperAdmin = false
                };

                var result = await userManager.CreateAsync(testUser, "TestUser@123");
                // Note: Not adding any roles to these test users
            }
        }
    }

    private static async Task SeedMenusAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
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
                Action = "Index",
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
        var blogMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Blog");
        if (blogMenu != null)
        {
            var blogSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "BlogPosts",
                    DisplayName = "Blog Posts",
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
                    Name = "BlogComments",
                    DisplayName = "Comments",
                    Controller = "BlogComment",
                    Action = "Index",
                    Icon = "fas fa-comments",
                    ParentId = blogMenu.Id,
                    Order = 4,
                    IsActive = true
                }
            };
            
            await context.Menus.AddRangeAsync(blogSubMenus);
            await context.SaveChangesAsync();
        }

        // Add User Management sub-menus
        var userMgmtMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "UserManagement");
        if (userMgmtMenu != null)
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
                    ParentId = userMgmtMenu.Id,
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
                    ParentId = userMgmtMenu.Id,
                    Order = 2,
                    IsActive = true
                },
                new Menu
                {
                    Name = "Permissions",
                    DisplayName = "Permissions",
                    Controller = "Menu",
                    Action = "Index",
                    Icon = "fas fa-key",
                    ParentId = userMgmtMenu.Id,
                    Order = 3,
                    IsActive = true
                }
            };
            
            await context.Menus.AddRangeAsync(userSubMenus);
            await context.SaveChangesAsync();
        }

        // Add Settings sub-menus
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
                    Icon = "fas fa-sliders-h",
                    ParentId = settingsMenu.Id,
                    Order = 1,
                    IsActive = true
                }
            };
            
            await context.Menus.AddRangeAsync(settingsSubMenus);
            await context.SaveChangesAsync();
        }

        // Add Customer Support sub-menus
        var supportMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "CustomerSupport");
        if (supportMenu != null)
        {
            var supportSubMenus = new List<Menu>
            {
                new Menu
                {
                    Name = "SupportDashboard",
                    DisplayName = "Support Dashboard",
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
                    Name = "Tickets",
                    DisplayName = "Tickets",
                    Area = "CustomerSupport",
                    Controller = "Ticket",
                    Action = "Index",
                    Icon = "fas fa-ticket-alt",
                    ParentId = supportMenu.Id,
                    Order = 2,
                    IsActive = true
                },
                new Menu
                {
                    Name = "ManageTickets",
                    DisplayName = "Manage Tickets",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Index",
                    Icon = "fas fa-tasks",
                    ParentId = supportMenu.Id,
                    Order = 3,
                    IsActive = true
                }
            };
            
            await context.Menus.AddRangeAsync(supportSubMenus);
            await context.SaveChangesAsync();
        }

        // Assign permissions
        await AssignMenuPermissionsAsync(context, roleManager);
    }

    private static async Task AssignMenuPermissionsAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var adminRole = await roleManager.FindByNameAsync("Administrator");
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        
        if (adminRole == null || superAdminRole == null)
            return;

        // Get all menus
        var allMenus = await context.Menus.ToListAsync();
        
        // Admin gets only Blog menus
        var blogMenus = allMenus.Where(m => 
            m.Name == "Blog" || 
            m.Name == "BlogPosts" || 
            m.Name == "BlogCategories" || 
            m.Name == "BlogTags" || 
            m.Name == "BlogComments" ||
            m.Name == "Dashboard"
        ).ToList();

        foreach (var menu in blogMenus)
        {
            var existingRoleMenu = await context.RoleMenus
                .FirstOrDefaultAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == menu.Id);
            
            if (existingRoleMenu == null)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = adminRole.Id,
                    MenuId = menu.Id,
                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.RoleMenus.Add(roleMenu);
            }
        }

        // SuperAdmin gets ALL menus (but we don't need to assign explicitly due to IsSuperAdmin flag)
        // Still adding for consistency
        foreach (var menu in allMenus)
        {
            var existingRoleMenu = await context.RoleMenus
                .FirstOrDefaultAsync(rm => rm.RoleId == superAdminRole.Id && rm.MenuId == menu.Id);
            
            if (existingRoleMenu == null)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = superAdminRole.Id,
                    MenuId = menu.Id,
                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.RoleMenus.Add(roleMenu);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedBlogCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.BlogCategories.AnyAsync())
            return;

        var categories = new List<BlogCategory>
        {
            new BlogCategory
            {
                Name = "Technology",
                Slug = "technology",
                Description = "Technology related posts",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "Business",
                Slug = "business",
                Description = "Business and entrepreneurship",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "Tutorial",
                Slug = "tutorial",
                Description = "How-to guides and tutorials",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.BlogCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        if (await context.SiteSettings.AnyAsync())
            return;

        var settings = SiteSettingsSeeder.GetDefaultSettings();
        await context.SiteSettings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedCustomerServiceRoleMappingsAsync(ApplicationDbContext context)
    {
        // Check if mappings already exist
        if (await context.CustomerServiceRoleMappings.AnyAsync())
            return;

        var mappings = new List<CustomerServiceRoleMapping>
        {
            new CustomerServiceRoleMapping
            {
                CustomerServiceRole = "CustomerSupportAdmin",
                AspNetRoleName = "CustomerSupportAdmin",
                Description = "Full administrative access to Customer Service area",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CustomerServiceRoleMapping
            {
                CustomerServiceRole = "CustomerSupportManager",
                AspNetRoleName = "CustomerSupportManager",
                Description = "Manager access - can view all tickets and assign work",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CustomerServiceRoleMapping
            {
                CustomerServiceRole = "CustomerSupportAgent",
                AspNetRoleName = "CustomerSupportAgent",
                Description = "Agent access - can view and respond to assigned tickets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CustomerServiceRoleMapping
            {
                CustomerServiceRole = "CustomerSupportCustomer",
                AspNetRoleName = "CustomerSupportCustomer",
                Description = "Customer access - can only view their own tickets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.CustomerServiceRoleMappings.AddRangeAsync(mappings);
        await context.SaveChangesAsync();
    }
}