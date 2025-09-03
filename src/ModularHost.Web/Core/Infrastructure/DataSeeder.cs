using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Models.Entities;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Core.Extensions;

namespace MRCMS.Core.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Users
            await SeedUsersAsync(userManager);

            // Seed Menus
            await SeedMenusAsync(context);

            // Seed Role Permissions
            await SeedRolePermissionsAsync(context, roleManager);

            // Seed Blog Categories
            await SeedBlogCategoriesAsync(context);

            // Seed Blog Tags
            await SeedBlogTagsAsync(context);

            // Seed Blog Posts
            await SeedBlogPostsAsync(context, userManager);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            var roles = new[]
            {
                new Role { Name = "SuperAdmin", Description = "Full system access", IsActive = true },
                new Role { Name = "Admin", Description = "Administrative access", IsActive = true },
                new Role { Name = "User", Description = "Regular user access", IsActive = true }
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            // Seed SuperAdmin
            if (await userManager.FindByEmailAsync("superadmin@example.com") == null)
            {
                var superAdmin = new User
                {
                    UserName = "superadmin",
                    Email = "superadmin@example.com",
                    EmailConfirmed = true,
                    FullName = "Super Admin",
                    IsSuperAdmin = true,
                    IsActive = true,
                    Address = "",
                    City = "",
                    Country = "",
                    PostalCode = "",
                    ProfilePicture = "/img/default-avatar.png",
                    RefreshToken = "",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            // Seed Admin
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FullName = "Admin User",
                    IsActive = true,
                    Address = "",
                    City = "",
                    Country = "",
                    PostalCode = "",
                    ProfilePicture = "/img/default-avatar.png",
                    RefreshToken = "",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Seed TestUser
            if (await userManager.FindByEmailAsync("testuser@example.com") == null)
            {
                var testUser = new User
                {
                    UserName = "testuser",
                    Email = "testuser@example.com",
                    EmailConfirmed = true,
                    FullName = "Test User",
                    IsActive = true,
                    Address = "",
                    City = "",
                    Country = "",
                    PostalCode = "",
                    ProfilePicture = "/img/default-avatar.png",
                    RefreshToken = "",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(testUser, "TestUser123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "User");
                }
            }
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext context, RoleManager<Role> roleManager)
        {
            if (!context.RolePermissions.Any())
            {
                var adminRole = await roleManager.FindByNameAsync("Admin");
                var userRole = await roleManager.FindByNameAsync("User");
                
                if (adminRole != null)
                {
                    var adminPermissions = new[]
                    {
                        // Blog Module Permissions
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Create",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Create",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Edit/*",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Edit/*",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Delete/*",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Categories",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/category/*",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Tags",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/tag/*",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/Comments",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Blog/comment/*",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        },
                        // User Management Permissions
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/User",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/User/*",
                            HttpMethod = "*",
                            Description = "Full access",
                            CreatedAt = DateTime.UtcNow
                        },
                        // Role Management Permissions
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Role",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id,
                            Url = "/Role/*",
                            HttpMethod = "*",
                            Description = "Full access",
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    
                    context.RolePermissions.AddRange(adminPermissions);
                }
                
                if (userRole != null)
                {
                    var userPermissions = new[]
                    {
                        // User can view blog posts
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = userRole.Id,
                            Url = "/Blog",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = userRole.Id,
                            Url = "/Blog/Details/*",
                            HttpMethod = "GET",
                            Description = "Access blog posts",
                            CreatedAt = DateTime.UtcNow
                        },
                        // User can post comments
                        new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = userRole.Id,
                            Url = "/Blog/comment/add",
                            HttpMethod = "POST",
                            Description = "Manage blog content",
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    
                    context.RolePermissions.AddRange(userPermissions);
                }
                
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedMenusAsync(AppDbContext context)
        {
            if (!context.Menus.Any())
            {
                // Create parent menus first
                var dashboardMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Dashboard",
                    Url = "/",
                    ActiveUrl = "/",
                    Icon = "fas fa-dashboard",
                    Order = 1,
                    IsVisible = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var blogMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Blog",
                    Url = "#",
                    ActiveUrl = "/Blog",
                    Icon = "fas fa-blog",
                    Order = 2,
                    IsVisible = true,
                    IsActive = true,
                    ModuleName = "BlogModule",
                    CreatedAt = DateTime.UtcNow
                };

                var adminMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Admin",
                    Url = "#",
                    ActiveUrl = "/Admin",
                    Icon = "fas fa-shield-alt",
                    Order = 3,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var reportsMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Reports",
                    Url = "#",
                    ActiveUrl = "/Reports",
                    Icon = "fas fa-chart-bar",
                    Order = 4,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var settingsMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Settings",
                    Url = "#",
                    ActiveUrl = "/Settings",
                    Icon = "fas fa-cog",
                    Order = 5,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var modulesMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Modules",
                    Url = "/Module",
                    ActiveUrl = "/Module",
                    Icon = "fas fa-puzzle-piece",
                    Order = 6,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "SuperAdmin",
                    CreatedAt = DateTime.UtcNow
                };

                var toolsMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Tools",
                    Url = "#",
                    ActiveUrl = "/Tools",
                    Icon = "fas fa-tools",
                    Order = 7,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var contentMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Content",
                    Url = "#",
                    ActiveUrl = "/Content",
                    Icon = "fas fa-file-alt",
                    Order = 8,
                    IsVisible = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Menus.AddRange(dashboardMenu, blogMenu, adminMenu, reportsMenu, settingsMenu, modulesMenu, toolsMenu, contentMenu);
                await context.SaveChangesAsync();

                // Add Blog submenu items
                var blogSubmenus = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "All Posts",
                        Url = "/Blog",
                        ActiveUrl = "/Blog",
                        Icon = "fas fa-list",
                        ParentId = blogMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Create Post",
                        Url = "/Blog/Create",
                        Icon = "fas fa-plus",
                        ParentId = blogMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Categories",
                        Url = "/Category",
                        ActiveUrl = "/Category",
                        Icon = "fas fa-folder",
                        ParentId = blogMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Tags",
                        Url = "/Tag",
                        ActiveUrl = "/Tag",
                        Icon = "fas fa-tags",
                        ParentId = blogMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Comments Management",
                        Url = "/Blog/Comments",
                        Icon = "fas fa-comments",
                        ParentId = blogMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Media Library",
                        Url = "/Blog/Media",
                        Icon = "fas fa-photo-video",
                        ParentId = blogMenu.Id,
                        Order = 6,
                        IsVisible = true,
                    IsActive = true,
                        ModuleName = "BlogModule",
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Menus.AddRange(blogSubmenus);
                
                // Add Admin submenu items
                var adminDashboard = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Dashboard",
                    Url = "/Admin",
                    Icon = "fas fa-tachometer-alt",
                    ParentId = adminMenu.Id,
                    Order = 1,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var usersMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Users",
                    Url = "/User",
                    ActiveUrl = "/User",
                    Icon = "fas fa-users",
                    ParentId = adminMenu.Id,
                    Order = 2,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var rolesMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Roles",
                    Url = "/Role",
                    ActiveUrl = "/Role",
                    Icon = "fas fa-user-shield",
                    ParentId = adminMenu.Id,
                    Order = 3,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                // Add a nested parent menu for Security Management
                var securityMenu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Security",
                    Url = "#", // This is a parent menu inside Admin menu
                    ActiveUrl = "/Admin/Security",
                    Icon = "fas fa-shield-alt",
                    ParentId = adminMenu.Id,
                    Order = 7,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var adminSubmenus = new[]
                {
                    adminDashboard,
                    usersMenu,
                    rolesMenu,
                    securityMenu,
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Menus",
                        Url = "/Menu",
                        ActiveUrl = "/Menu",
                        Icon = "fas fa-bars",
                        ParentId = adminMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Permissions",
                        Url = "/Admin/Permissions",
                        Icon = "fas fa-lock",
                        ParentId = adminMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Audit Log",
                        Url = "/Admin/AuditLog",
                        Icon = "fas fa-history",
                        ParentId = adminMenu.Id,
                        Order = 6,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "System Monitor",
                        Url = "/Admin/SystemMonitor",
                        Icon = "fas fa-desktop",
                        ParentId = adminMenu.Id,
                        Order = 7,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(adminSubmenus);
                await context.SaveChangesAsync();
                
                // Add grandchildren for Users menu (3rd level)
                var userGrandchildren = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "All Users",
                        Url = "/User",
                        Icon = "fas fa-list",
                        ParentId = usersMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Create User",
                        Url = "/User/Create",
                        Icon = "fas fa-user-plus",
                        ParentId = usersMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "User Groups",
                        Url = "/User/Groups",
                        Icon = "fas fa-users-cog",
                        ParentId = usersMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "User Import/Export",
                        Url = "/User/ImportExport",
                        Icon = "fas fa-file-import",
                        ParentId = usersMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(userGrandchildren);
                
                // Add grandchildren for Roles menu (3rd level)
                var roleGrandchildren = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "All Roles",
                        Url = "/Role",
                        Icon = "fas fa-list",
                        ParentId = rolesMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Create Role",
                        Url = "/Role/Create",
                        Icon = "fas fa-plus-circle",
                        ParentId = rolesMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Role Templates",
                        Url = "/Role/Templates",
                        Icon = "fas fa-file-code",
                        ParentId = rolesMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(roleGrandchildren);
                
                // Add children for Security menu (nested parent menu test)
                var securityChildren = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Two-Factor Auth",
                        Url = "/Admin/Security/TwoFactor",
                        Icon = "fas fa-mobile-alt",
                        ParentId = securityMenu.Id,
                        Order = 1,
                        IsVisible = true,
                        IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Session Management",
                        Url = "/Admin/Security/Sessions",
                        Icon = "fas fa-clock",
                        ParentId = securityMenu.Id,
                        Order = 2,
                        IsVisible = true,
                        IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "IP Restrictions",
                        Url = "/Admin/Security/IPRestrictions",
                        Icon = "fas fa-network-wired",
                        ParentId = securityMenu.Id,
                        Order = 3,
                        IsVisible = true,
                        IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(securityChildren);
                
                // Add Settings submenu items
                var settingsOverview = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "Settings Overview",
                    Url = "/Settings",
                    Icon = "fas fa-cog",
                    ParentId = settingsMenu.Id,
                    Order = 1,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var generalSettings = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = "General Settings",
                    Url = "/Settings/General",
                    Icon = "fas fa-sliders-h",
                    ParentId = settingsMenu.Id,
                    Order = 2,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var settingsSubmenus = new[]
                {
                    settingsOverview,
                    generalSettings,
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Email Settings",
                        Url = "/Settings/Email",
                        Icon = "fas fa-envelope",
                        ParentId = settingsMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Security Settings",
                        Url = "/Settings/Security",
                        Icon = "fas fa-shield-alt",
                        ParentId = settingsMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Database Settings",
                        Url = "/Settings/Database",
                        Icon = "fas fa-database",
                        ParentId = settingsMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Cache Management",
                        Url = "/Settings/Cache",
                        Icon = "fas fa-memory",
                        ParentId = settingsMenu.Id,
                        Order = 6,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "File Manager",
                        Url = "/Settings/FileManager",
                        Icon = "fas fa-folder",
                        ParentId = settingsMenu.Id,
                        Order = 7,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(settingsSubmenus);
                await context.SaveChangesAsync();
                
                // Add great-grandchildren for General Settings (4th level) as example
                var generalSettingsChildren = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Site Information",
                        Url = "/Settings/General/Site",
                        Icon = "fas fa-info-circle",
                        ParentId = generalSettings.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Localization",
                        Url = "/Settings/General/Localization",
                        Icon = "fas fa-globe",
                        ParentId = generalSettings.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Date & Time",
                        Url = "/Settings/General/DateTime",
                        Icon = "fas fa-clock",
                        ParentId = generalSettings.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(generalSettingsChildren);
                
                // Add Reports submenu items
                var reportsSubmenus = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Reports Dashboard",
                        Url = "/Reports",
                        Icon = "fas fa-chart-bar",
                        ParentId = reportsMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "User Activity Report",
                        Url = "/Reports/UserActivity",
                        Icon = "fas fa-user-clock",
                        ParentId = reportsMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Blog Statistics",
                        Url = "/Reports/BlogStats",
                        Icon = "fas fa-chart-line",
                        ParentId = reportsMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        ModuleName = "BlogModule",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "System Performance",
                        Url = "/Reports/Performance",
                        Icon = "fas fa-tachometer-alt",
                        ParentId = reportsMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Security Reports",
                        Url = "/Reports/Security",
                        Icon = "fas fa-shield-alt",
                        ParentId = reportsMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "System Logs",
                        Url = "/Reports/SystemLogs",
                        Icon = "fas fa-file-alt",
                        ParentId = reportsMenu.Id,
                        Order = 6,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Export Data",
                        Url = "/Reports/Export",
                        Icon = "fas fa-download",
                        ParentId = reportsMenu.Id,
                        Order = 7,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(reportsSubmenus);
                
                // Add Tools submenu items
                var toolsSubmenus = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Database Migration",
                        Url = "/Tools/Migration",
                        Icon = "fas fa-database",
                        ParentId = toolsMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Cache Manager",
                        Url = "/Tools/Cache",
                        Icon = "fas fa-memory",
                        ParentId = toolsMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Log Viewer",
                        Url = "/Tools/Logs",
                        Icon = "fas fa-file-alt",
                        ParentId = toolsMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Backup Manager",
                        Url = "/Tools/Backup",
                        Icon = "fas fa-archive",
                        ParentId = toolsMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Import/Export",
                        Url = "/Tools/ImportExport",
                        Icon = "fas fa-exchange-alt",
                        ParentId = toolsMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "System Health",
                        Url = "/Tools/Health",
                        Icon = "fas fa-heartbeat",
                        ParentId = toolsMenu.Id,
                        Order = 6,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(toolsSubmenus);
                
                // Add Content submenu items
                var contentSubmenus = new[]
                {
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Pages",
                        Url = "/Content/Pages",
                        Icon = "fas fa-file",
                        ParentId = contentMenu.Id,
                        Order = 1,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Media Library",
                        Url = "/Content/Media",
                        Icon = "fas fa-photo-video",
                        ParentId = contentMenu.Id,
                        Order = 2,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "File Manager",
                        Url = "/Content/Files",
                        Icon = "fas fa-folder-open",
                        ParentId = contentMenu.Id,
                        Order = 3,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Templates",
                        Url = "/Content/Templates",
                        Icon = "fas fa-code",
                        ParentId = contentMenu.Id,
                        Order = 4,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Title = "Widgets",
                        Url = "/Content/Widgets",
                        Icon = "fas fa-cube",
                        ParentId = contentMenu.Id,
                        Order = 5,
                        IsVisible = true,
                    IsActive = true,
                        ClaimType = "Admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.Menus.AddRange(contentSubmenus);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBlogCategoriesAsync(AppDbContext context)
        {
            
            if (!context.Set<Category>().Any())
            {
                var categories = new[]
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Technology",
                        Slug = "technology",
                        Description = "Technology related posts",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Programming",
                        Slug = "programming",
                        Description = "Programming tutorials and tips",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Web Development",
                        Slug = "web-development",
                        Description = "Web development articles",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Mobile Development",
                        Slug = "mobile-development",
                        Description = "Mobile app development",
                        DisplayOrder = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "News",
                        Slug = "news",
                        Description = "Latest news and updates",
                        DisplayOrder = 5,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Set<Category>().AddRange(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBlogTagsAsync(AppDbContext context)
        {
            if (!context.Set<Tag>().Any())
            {
                var tags = new[]
                {
                    new Tag { Id = Guid.NewGuid(), Name = "C#", Slug = "csharp", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "ASP.NET Core", Slug = "aspnet-core", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Entity Framework", Slug = "entity-framework", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "JavaScript", Slug = "javascript", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "TypeScript", Slug = "typescript", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "React", Slug = "react", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Angular", Slug = "angular", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Vue.js", Slug = "vuejs", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Docker", Slug = "docker", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Kubernetes", Slug = "kubernetes", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Azure", Slug = "azure", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "AWS", Slug = "aws", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "Microservices", Slug = "microservices", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "REST API", Slug = "rest-api", CreatedAt = DateTime.UtcNow },
                    new Tag { Id = Guid.NewGuid(), Name = "GraphQL", Slug = "graphql", CreatedAt = DateTime.UtcNow }
                };

                context.Set<Tag>().AddRange(tags);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBlogPostsAsync(AppDbContext context, UserManager<User> userManager)
        {
            if (!context.Set<BlogPost>().Any())
            {
                var admin = await userManager.FindByEmailAsync("admin@example.com");
                var techCategory = await context.Set<Category>().FirstAsync(c => c.Slug == "technology");
                var progCategory = await context.Set<Category>().FirstAsync(c => c.Slug == "programming");
                var webCategory = await context.Set<Category>().FirstAsync(c => c.Slug == "web-development");
                
                var csharpTag = await context.Set<Tag>().FirstAsync(t => t.Slug == "csharp");
                var aspnetTag = await context.Set<Tag>().FirstAsync(t => t.Slug == "aspnet-core");
                var efTag = await context.Set<Tag>().FirstAsync(t => t.Slug == "entity-framework");

                var posts = new[]
                {
                    new BlogPost
                    {
                        Id = Guid.NewGuid(),
                        Title = "Getting Started with ASP.NET Core MVC",
                        Slug = "getting-started-aspnet-core-mvc",
                        Summary = "Learn the basics of building web applications with ASP.NET Core MVC framework.",
                        Content = @"<h2>Introduction</h2>
                        <p>ASP.NET Core MVC is a rich framework for building web apps and APIs using the Model-View-Controller design pattern.</p>
                        <h3>What is MVC?</h3>
                        <p>The Model-View-Controller (MVC) architectural pattern separates an application into three main groups of components: Models, Views, and Controllers.</p>
                        <ul>
                            <li><strong>Model:</strong> Represents the data and business logic</li>
                            <li><strong>View:</strong> Handles the presentation layer</li>
                            <li><strong>Controller:</strong> Handles user input and interactions</li>
                        </ul>
                        <h3>Setting Up Your First Project</h3>
                        <p>To create a new ASP.NET Core MVC project, you can use the .NET CLI:</p>
                        <pre><code>dotnet new mvc -n MyMvcApp</code></pre>",
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-10),
                        AuthorId = admin.Id,
                        CategoryId = webCategory.Id,
                        ViewCount = 150,
                        FeaturedImage = "/images/blog/aspnet-core-mvc.jpg",
                        MetaTitle = "Getting Started with ASP.NET Core MVC - Tutorial",
                        MetaDescription = "Learn ASP.NET Core MVC basics",
                        MetaKeywords = "ASP.NET Core, MVC, Web Development",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = admin.Id
                    },
                    new BlogPost
                    {
                        Id = Guid.NewGuid(),
                        Title = "Understanding Entity Framework Core",
                        Slug = "understanding-entity-framework-core",
                        Summary = "A comprehensive guide to Entity Framework Core and its features.",
                        Content = @"<h2>What is Entity Framework Core?</h2>
                        <p>Entity Framework Core is a modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations.</p>
                        <h3>Key Features</h3>
                        <ul>
                            <li>Cross-platform</li>
                            <li>Lightweight and extensible</li>
                            <li>Built-in dependency injection</li>
                            <li>Support for multiple database providers</li>
                        </ul>",
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-5),
                        AuthorId = admin.Id,
                        CategoryId = progCategory.Id,
                        ViewCount = 89,
                        FeaturedImage = "/images/blog/entity-framework.jpg",
                        MetaTitle = "Understanding Entity Framework Core - Complete Guide",
                        MetaDescription = "Complete guide to Entity Framework Core",
                        MetaKeywords = "Entity Framework, EF Core, ORM, Database",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedBy = admin.Id
                    },
                    new BlogPost
                    {
                        Id = Guid.NewGuid(),
                        Title = "Building Modular Applications in ASP.NET Core",
                        Slug = "building-modular-applications-aspnet-core",
                        Summary = "Learn how to create modular and maintainable applications using ASP.NET Core.",
                        Content = @"<h2>Why Modular Architecture?</h2>
                        <p>Modular architecture helps in building scalable and maintainable applications by separating concerns into independent modules.</p>
                        <h3>Benefits</h3>
                        <ul>
                            <li>Better code organization</li>
                            <li>Easier testing and debugging</li>
                            <li>Independent deployment</li>
                            <li>Team collaboration</li>
                        </ul>",
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-2),
                        AuthorId = admin.Id,
                        CategoryId = techCategory.Id,
                        ViewCount = 45,
                        FeaturedImage = "/images/blog/modular-architecture.jpg",
                        MetaTitle = "Building Modular Applications - ASP.NET Core",
                        MetaDescription = "Guide to building modular ASP.NET Core applications",
                        MetaKeywords = "Modular Architecture, ASP.NET Core, Clean Architecture",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedBy = admin.Id
                    }
                };

                context.Set<BlogPost>().AddRange(posts);
                await context.SaveChangesAsync();

                // Add tags to posts
                var post1 = await context.Set<BlogPost>().FirstAsync(p => p.Slug == "getting-started-aspnet-core-mvc");
                var post2 = await context.Set<BlogPost>().FirstAsync(p => p.Slug == "understanding-entity-framework-core");
                var post3 = await context.Set<BlogPost>().FirstAsync(p => p.Slug == "building-modular-applications-aspnet-core");

                var postTags = new[]
                {
                    new BlogPostTag { BlogPostId = post1.Id, TagId = aspnetTag.Id },
                    new BlogPostTag { BlogPostId = post1.Id, TagId = csharpTag.Id },
                    new BlogPostTag { BlogPostId = post2.Id, TagId = efTag.Id },
                    new BlogPostTag { BlogPostId = post2.Id, TagId = csharpTag.Id },
                    new BlogPostTag { BlogPostId = post3.Id, TagId = aspnetTag.Id },
                    new BlogPostTag { BlogPostId = post3.Id, TagId = csharpTag.Id }
                };

                context.Set<BlogPostTag>().AddRange(postTags);
                await context.SaveChangesAsync();
            }
        }
    }
}