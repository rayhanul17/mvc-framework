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
        
        // Seed site settings
        await SeedSiteSettingsAsync(context);
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
                    },
                    new Menu
                    {
                        Name = "Site Settings",
                        DisplayName = "Site Settings",
                        Controller = "SiteSetting",
                        Action = "Index",
                        Icon = "fas fa-cog",
                        ParentId = 5,
                        Order = 4,
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
    
    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        if (!await context.SiteSettings.AnyAsync())
        {
            var settings = new List<SiteSetting>
            {
                // Branding Settings
                new SiteSetting
                {
                    Key = "Site.Name",
                    Value = "Dynamic Role Menu System",
                    Description = "The main site/application name",
                    Category = SettingCategory.Branding,
                    Type = SettingType.Text,
                    IsRequired = true,
                    IsSystemSetting = false,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Site.Description",
                    Value = "A comprehensive role-based menu management system built with ASP.NET Core",
                    Description = "Site description or slogan",
                    Category = SettingCategory.Branding,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Organization.Name",
                    Value = "Your Organization",
                    Description = "Organization or company name",
                    Category = SettingCategory.Branding,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Site.Logo",
                    Value = "/images/logo.png",
                    Description = "Path to the site logo image",
                    Category = SettingCategory.Branding,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Site.Favicon",
                    Value = "/favicon.ico",
                    Description = "Path to the site favicon",
                    Category = SettingCategory.Branding,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 5,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Theme Settings
                new SiteSetting
                {
                    Key = "Theme.PrimaryColor",
                    Value = "#0d6efd",
                    Description = "Primary theme color (Bootstrap primary)",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Color,
                    IsRequired = true,
                    IsSystemSetting = false,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Theme.SecondaryColor",
                    Value = "#6c757d",
                    Description = "Secondary theme color",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Color,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Theme.SuccessColor",
                    Value = "#198754",
                    Description = "Success theme color",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Color,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Theme.DangerColor",
                    Value = "#dc3545",
                    Description = "Danger/error theme color",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Color,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Theme.WarningColor",
                    Value = "#ffc107",
                    Description = "Warning theme color",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Color,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Theme.DarkMode",
                    Value = "false",
                    Description = "Enable dark mode by default",
                    Category = SettingCategory.Theme,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 6,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Layout Settings (Footer)
                new SiteSetting
                {
                    Key = "Footer.CompanyName",
                    Value = "Dynamic Role Menu System",
                    Description = "Company name displayed in footer",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.CopyrightYear",
                    Value = DateTime.UtcNow.Year.ToString(),
                    Description = "Copyright year displayed in footer",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.ShowPoweredBy",
                    Value = "true",
                    Description = "Show 'Powered by' text in footer",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.CustomText",
                    Value = "",
                    Description = "Additional custom text to display in footer",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.Text",
                    Value = "Building innovative solutions for modern businesses. We are committed to delivering high-quality software that helps organizations streamline their operations and achieve their goals.",
                    Description = "Main footer text/description",
                    Category = SettingCategory.Layout,
                    Type = SettingType.TextArea,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.CopyrightText",
                    Value = "",
                    Description = "Custom copyright text (leave empty for auto-generated)",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 6,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.ShowSocialLinks",
                    Value = "true",
                    Description = "Show social media links in footer",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 7,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Footer.ShowMenu",
                    Value = "true",
                    Description = "Show footer menu/quick links",
                    Category = SettingCategory.Layout,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 8,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Social Media Settings
                new SiteSetting
                {
                    Key = "Social.Facebook",
                    Value = "https://facebook.com/yourcompany",
                    Description = "Facebook page URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 10,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Social.Twitter",
                    Value = "https://twitter.com/yourcompany",
                    Description = "Twitter/X profile URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 11,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Social.LinkedIn",
                    Value = "https://linkedin.com/company/yourcompany",
                    Description = "LinkedIn company page URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 12,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Social.Instagram",
                    Value = "",
                    Description = "Instagram profile URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 13,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Social.YouTube",
                    Value = "",
                    Description = "YouTube channel URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 14,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Social.GitHub",
                    Value = "https://github.com/yourcompany",
                    Description = "GitHub organization/profile URL",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 15,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Contact Settings
                new SiteSetting
                {
                    Key = "Contact.Email",
                    Value = "admin@example.com",
                    Description = "Primary contact email address",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Email,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Contact.Phone",
                    Value = "+1-555-0123",
                    Description = "Primary contact phone number",
                    Category = SettingCategory.Contact,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "Contact.Address",
                    Value = "123 Main Street, City, State 12345",
                    Description = "Physical address",
                    Category = SettingCategory.Contact,
                    Type = SettingType.TextArea,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Advanced Settings (SEO)
                new SiteSetting
                {
                    Key = "SEO.MetaTitle",
                    Value = "Dynamic Role Menu System",
                    Description = "Default meta title for pages",
                    Category = SettingCategory.Advanced,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "SEO.MetaDescription",
                    Value = "A comprehensive role-based menu management system built with ASP.NET Core MVC framework",
                    Description = "Default meta description for pages",
                    Category = SettingCategory.Advanced,
                    Type = SettingType.TextArea,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "SEO.MetaKeywords",
                    Value = "role management, menu system, asp.net core, mvc, authorization",
                    Description = "Default meta keywords for pages",
                    Category = SettingCategory.Advanced,
                    Type = SettingType.Text,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Security Settings
                new SiteSetting
                {
                    Key = "System.Version",
                    Value = "1.0.0",
                    Description = "Current system version",
                    Category = SettingCategory.Security,
                    Type = SettingType.Text,
                    IsRequired = true,
                    IsSystemSetting = true,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "System.MaintenanceMode",
                    Value = "false",
                    Description = "Enable maintenance mode",
                    Category = SettingCategory.Security,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "System.AllowRegistration",
                    Value = "false",
                    Description = "Allow user registration",
                    Category = SettingCategory.Security,
                    Type = SettingType.Boolean,
                    IsRequired = false,
                    IsSystemSetting = false,
                    Order = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new SiteSetting
                {
                    Key = "System.DefaultUserRole",
                    Value = "User",
                    Description = "Default role assigned to new users",
                    Category = SettingCategory.Security,
                    Type = SettingType.Text,
                    IsRequired = true,
                    IsSystemSetting = false,
                    Order = 4,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.SiteSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }
}