using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Infrastructure.Data;

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

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed admin user
        await SeedAdminUserAsync(userManager);

        // Seed menus
        await SeedMenusAsync(context, roleManager);
        
        // Seed blog categories
        await SeedBlogCategoriesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { "SuperAdmin", "Administrator", "Manager", "Editor", "User" };
        
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role with default permissions",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed SuperAdmin user
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
                IsActive = true
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }

        // Seed regular Admin user
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                Description = "Default system administrator account",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }

    private static async Task SeedMenusAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        if (!await context.Menus.AnyAsync())
        {
            var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
            
            if (superAdminRole != null)
            {
                var menus = new List<Menu>
                {
                    new Menu
                    {
                        Name = "Dashboard",
                        DisplayName = "Dashboard",
                        Controller = "Home",
                        Action = "Index",
                        Icon = "fas fa-tachometer-alt",
                        Order = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Blog Management",
                        DisplayName = "Blog Management",
                        Icon = "fas fa-blog",
                        Order = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Blog Categories",
                        DisplayName = "Blog Categories",
                        Controller = "BlogCategory",
                        Action = "Index",
                        Icon = "fas fa-folder",
                        ParentId = 2,
                        Order = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Blog Posts",
                        DisplayName = "Blog Posts",
                        Controller = "BlogPost",
                        Action = "Index",
                        Icon = "fas fa-newspaper",
                        ParentId = 2,
                        Order = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Administration",
                        DisplayName = "Administration",
                        Icon = "fas fa-cogs",
                        Order = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "User Management",
                        DisplayName = "User Management",
                        Controller = "User",
                        Action = "Index",
                        Icon = "fas fa-users",
                        ParentId = 5,
                        Order = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Role Management",
                        DisplayName = "Role Management",
                        Controller = "Role",
                        Action = "Index",
                        Icon = "fas fa-user-tag",
                        ParentId = 5,
                        Order = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Menu Management",
                        DisplayName = "Menu Management",
                        Controller = "Menu",
                        Action = "Index",
                        Icon = "fas fa-bars",
                        ParentId = 5,
                        Order = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                // First, add parent menus
                var parentMenus = menus.Where(m => m.ParentId == null).ToList();
                await context.Menus.AddRangeAsync(parentMenus);
                await context.SaveChangesAsync();

                // Then add child menus with correct parent IDs
                var blogMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Blog Management");
                if (blogMenu != null)
                {
                    var blogChildMenus = menus.Where(m => m.ParentId == 2).ToList();
                    foreach (var childMenu in blogChildMenus)
                    {
                        childMenu.ParentId = blogMenu.Id;
                        await context.Menus.AddAsync(childMenu);
                    }
                }
                
                var adminMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Administration");
                if (adminMenu != null)
                {
                    var adminChildMenus = menus.Where(m => m.ParentId == 5).ToList();
                    foreach (var childMenu in adminChildMenus)
                    {
                        childMenu.ParentId = adminMenu.Id;
                        await context.Menus.AddAsync(childMenu);
                    }
                }
                await context.SaveChangesAsync();

                // Assign all menus to SuperAdmin role
                var allMenus = await context.Menus.ToListAsync();
                foreach (var menu in allMenus)
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
                    await context.RoleMenus.AddAsync(roleMenu);
                }

                // Assign only blog menus to Administrator role
                var adminRole = await roleManager.FindByNameAsync("Administrator");
                if (adminRole != null)
                {
                    var blogMenus = await context.Menus
                        .Where(m => m.Name == "Blog Management" || 
                                   m.Name == "Blog Categories" || 
                                   m.Name == "Blog Posts")
                        .ToListAsync();

                    foreach (var menu in blogMenus)
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
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
    
    private static async Task SeedBlogCategoriesAsync(ApplicationDbContext context)
    {
        if (!await context.BlogCategories.AnyAsync())
        {
            var categories = new List<BlogCategory>
            {
                new BlogCategory
                {
                    Name = "Technology",
                    Slug = "technology",
                    Description = "Posts about technology and software development",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new BlogCategory
                {
                    Name = "Business",
                    Slug = "business",
                    Description = "Business insights and strategies",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new BlogCategory
                {
                    Name = "Tutorial",
                    Slug = "tutorial",
                    Description = "Step-by-step guides and tutorials",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new BlogCategory
                {
                    Name = "News",
                    Slug = "news",
                    Description = "Latest news and updates",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.BlogCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}