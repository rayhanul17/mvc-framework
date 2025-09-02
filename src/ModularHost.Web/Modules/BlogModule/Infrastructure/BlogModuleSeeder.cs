using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Modules.Blog.Models.Entities;

namespace MRCMS.Modules.BlogModule.Infrastructure
{
    /// <summary>
    /// Seeder for Blog module specific data
    /// </summary>
    public class BlogModuleSeeder : ModuleSeederBase
    {
        public override string ModuleName => "BlogModule";
        public override int Order => 10; // Execute after core seeders

        protected override async Task SeedModuleDataAsync(
            IServiceProvider serviceProvider, 
            AppDbContext context, 
            UserManager<User> userManager, 
            RoleManager<Role> roleManager)
        {
            // Clean up existing blog module menus first
            await CleanupExistingBlogMenusAsync(context);

            // Seed Blog-specific menus
            await SeedBlogMenusAsync(context);

            // Seed Blog-specific permissions
            await SeedBlogPermissionsAsync(context);

            // Seed default blog settings if needed
            await SeedBlogSettingsAsync(context);

            Logger?.LogInformation("Blog module seeding completed");
        }

        private async Task CleanupExistingBlogMenusAsync(AppDbContext context)
        {
            // Remove existing blog module menus to allow fresh seeding
            var existingBlogMenus = await context.Menus
                .Where(m => m.ModuleName == ModuleName)
                .ToListAsync();

            if (existingBlogMenus.Any())
            {
                context.Menus.RemoveRange(existingBlogMenus);
                await context.SaveChangesAsync();
                Logger?.LogInformation($"Removed {existingBlogMenus.Count} existing blog menus");
            }

            // Also remove blog-specific report menus
            var blogReportMenus = await context.Menus
                .Where(m => m.ModuleName == ModuleName && m.Url != null && m.Url.StartsWith("/Reports/"))
                .ToListAsync();

            if (blogReportMenus.Any())
            {
                context.Menus.RemoveRange(blogReportMenus);
                await context.SaveChangesAsync();
                Logger?.LogInformation($"Removed {blogReportMenus.Count} existing blog report menus");
            }
        }

        protected override async Task<bool> CheckIfSeedingRequiredAsync(AppDbContext context)
        {
            // Check if any blog module specific data needs seeding
            var hasBlogMenus = await context.Menus.AnyAsync(m => m.ModuleName == ModuleName);
            var hasBlogCategories = await context.Categories.AnyAsync();
            var hasBlogTags = await context.Tags.AnyAsync();
            var hasBlogPosts = await context.BlogPosts.AnyAsync();
            
            // Seed if any of the blog data is missing
            return !hasBlogMenus || !hasBlogCategories || !hasBlogTags || !hasBlogPosts;
            
            // Check if blog menus exist
            // var blogMenuExists = await context.Menus
            //     .AnyAsync(m => m.ModuleName == ModuleName);

            // return !blogMenuExists;
        }

        private async Task SeedBlogMenusAsync(AppDbContext context)
        {
            // Get or create parent Blog menu
            var blogParentMenu = await GetOrCreateParentMenuAsync(
                context, 
                "Blog", 
                "#", 
                "fas fa-blog", 
                2);

            if (blogParentMenu == null) return;

            // Define comprehensive blog submenu items  
            var blogSubmenus = new[]
            {
                new Menu
                {
                    Title = "Blog Dashboard",
                    Url = "/Blog/Dashboard",
                    ActiveUrl = "/Blog/Dashboard",
                    Icon = "fas fa-tachometer-alt",
                    ParentId = blogParentMenu.Id,
                    Order = 1,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "All Posts",
                    Url = "/Blog",
                    ActiveUrl = "/Blog",
                    Icon = "fas fa-list",
                    ParentId = blogParentMenu.Id,
                    Order = 2,
                    IsVisible = true,
                    IsActive = true,
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Create Post",
                    Url = "/Blog/Create",
                    Icon = "fas fa-plus",
                    ParentId = blogParentMenu.Id,
                    Order = 3,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "My Posts",
                    Url = "/Blog/MyPosts",
                    Icon = "fas fa-user-edit",
                    ParentId = blogParentMenu.Id,
                    Order = 4,
                    IsVisible = true,
                    ClaimType = "User",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Categories",
                    Url = "/Category",
                    ActiveUrl = "/Category",
                    Icon = "fas fa-folder",
                    ParentId = blogParentMenu.Id,
                    Order = 5,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Tags",
                    Url = "/Tag",
                    ActiveUrl = "/Tag",
                    Icon = "fas fa-tags",
                    ParentId = blogParentMenu.Id,
                    Order = 6,
                    IsVisible = true,
                    IsActive = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Comments Management",
                    Url = "/Blog/Comments",
                    Icon = "fas fa-comments",
                    ParentId = blogParentMenu.Id,
                    Order = 7,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Media Library",
                    Url = "/Blog/Media",
                    Icon = "fas fa-photo-video",
                    ParentId = blogParentMenu.Id,
                    Order = 8,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Blog Analytics",
                    Url = "/Blog/Analytics",
                    Icon = "fas fa-chart-line",
                    ParentId = blogParentMenu.Id,
                    Order = 9,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "SEO Management",
                    Url = "/Blog/SEO",
                    Icon = "fas fa-search",
                    ParentId = blogParentMenu.Id,
                    Order = 10,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Blog Settings",
                    Url = "/Blog/Settings",
                    Icon = "fas fa-cog",
                    ParentId = blogParentMenu.Id,
                    Order = 11,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                }
            };

            await SeedModuleMenusAsync(context, blogSubmenus);

            // Also seed additional blog-related menus in Reports section
            await SeedBlogReportsMenusAsync(context);
        }

        private async Task SeedBlogReportsMenusAsync(AppDbContext context)
        {
            // Find existing Reports parent menu
            var reportsParentMenu = await context.Menus
                .FirstOrDefaultAsync(m => m.Title == "Reports" && m.ParentId == null);

            if (reportsParentMenu == null) return;

            // Define blog-specific report menus
            var blogReportSubmenus = new[]
            {
                new Menu
                {
                    Title = "Blog Statistics",
                    Url = "/Reports/BlogStats",
                    Icon = "fas fa-chart-line",
                    ParentId = reportsParentMenu.Id,
                    Order = 2,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Popular Posts",
                    Url = "/Reports/PopularPosts",
                    Icon = "fas fa-fire",
                    ParentId = reportsParentMenu.Id,
                    Order = 3,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Comment Analytics",
                    Url = "/Reports/CommentAnalytics",
                    Icon = "fas fa-comment-dots",
                    ParentId = reportsParentMenu.Id,
                    Order = 4,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "Author Performance",
                    Url = "/Reports/AuthorPerformance",
                    Icon = "fas fa-user-chart",
                    ParentId = reportsParentMenu.Id,
                    Order = 5,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                },
                new Menu
                {
                    Title = "SEO Reports",
                    Url = "/Reports/SEOReports",
                    Icon = "fas fa-search-plus",
                    ParentId = reportsParentMenu.Id,
                    Order = 6,
                    IsVisible = true,
                    ClaimType = "Admin",
                    ModuleName = ModuleName
                }
            };

            // Check if these menus already exist
            foreach (var menu in blogReportSubmenus)
            {
                var existingMenu = await context.Menus
                    .FirstOrDefaultAsync(m => m.Url == menu.Url && m.ParentId == reportsParentMenu.Id);

                if (existingMenu == null)
                {
                    menu.Id = Guid.NewGuid();
                    menu.CreatedAt = DateTime.UtcNow;
                    context.Menus.Add(menu);
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task SeedBlogPermissionsAsync(AppDbContext context)
        {
            // Define comprehensive blog-specific permissions for Admin role
            var adminPermissions = new[]
            {
                new RolePermission
                {
                    Url = "/Blog/Dashboard",
                    HttpMethod = "GET",
                    Description = "View blog dashboard and analytics"
                },
                new RolePermission
                {
                    Url = "/Blog",
                    HttpMethod = "*",
                    Description = "Full access to blog posts management"
                },
                new RolePermission
                {
                    Url = "/Blog/Create",
                    HttpMethod = "GET,POST",
                    Description = "Create new blog posts"
                },
                new RolePermission
                {
                    Url = "/Blog/Edit/*",
                    HttpMethod = "GET,POST",
                    Description = "Edit any blog posts"
                },
                new RolePermission
                {
                    Url = "/Blog/Delete/*",
                    HttpMethod = "POST,DELETE",
                    Description = "Delete any blog posts"
                },
                new RolePermission
                {
                    Url = "/Blog/MyPosts",
                    HttpMethod = "GET",
                    Description = "View user's own posts"
                },
                new RolePermission
                {
                    Url = "/Category*",
                    HttpMethod = "*",
                    Description = "Full access to category management"
                },
                new RolePermission
                {
                    Url = "/Tag*",
                    HttpMethod = "*",
                    Description = "Full access to tag management"
                },
                new RolePermission
                {
                    Url = "/Blog/Comments*",
                    HttpMethod = "*",
                    Description = "Full access to comment management"
                },
                new RolePermission
                {
                    Url = "/Blog/Media*",
                    HttpMethod = "*",
                    Description = "Full access to media library"
                },
                new RolePermission
                {
                    Url = "/Blog/Analytics*",
                    HttpMethod = "GET",
                    Description = "Access to blog analytics"
                },
                new RolePermission
                {
                    Url = "/Blog/SEO*",
                    HttpMethod = "*",
                    Description = "Access to SEO management"
                },
                new RolePermission
                {
                    Url = "/Blog/Settings*",
                    HttpMethod = "*",
                    Description = "Access to blog settings"
                },
                new RolePermission
                {
                    Url = "/Reports/BlogStats*",
                    HttpMethod = "GET",
                    Description = "Access to blog statistics reports"
                },
                new RolePermission
                {
                    Url = "/Reports/PopularPosts*",
                    HttpMethod = "GET",
                    Description = "Access to popular posts reports"
                },
                new RolePermission
                {
                    Url = "/Reports/CommentAnalytics*",
                    HttpMethod = "GET",
                    Description = "Access to comment analytics"
                },
                new RolePermission
                {
                    Url = "/Reports/AuthorPerformance*",
                    HttpMethod = "GET",
                    Description = "Access to author performance reports"
                },
                new RolePermission
                {
                    Url = "/Reports/SEOReports*",
                    HttpMethod = "GET",
                    Description = "Access to SEO reports"
                }
            };

            await SeedModulePermissionsAsync(context, "Admin", adminPermissions);

            // Define blog-specific permissions for BlogAuthor role (if exists)
            var blogAuthorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "BlogAuthor");
            if (blogAuthorRole != null)
            {
                var authorPermissions = new[]
                {
                    new RolePermission
                    {
                        Url = "/Blog",
                        HttpMethod = "GET",
                        Description = "View blog posts list"
                    },
                    new RolePermission
                    {
                        Url = "/Blog/Create",
                        HttpMethod = "GET,POST",
                        Description = "Create blog posts"
                    },
                    new RolePermission
                    {
                        Url = "/Blog/Edit/*",
                        HttpMethod = "GET,POST",
                        Description = "Edit own blog posts"
                    },
                    new RolePermission
                    {
                        Url = "/Blog/MyPosts",
                        HttpMethod = "GET",
                        Description = "View own posts"
                    },
                    new RolePermission
                    {
                        Url = "/Category",
                        HttpMethod = "GET",
                        Description = "View categories"
                    },
                    new RolePermission
                    {
                        Url = "/Tag",
                        HttpMethod = "GET",
                        Description = "View tags"
                    },
                    new RolePermission
                    {
                        Url = "/Blog/Media",
                        HttpMethod = "GET,POST",
                        Description = "Access media library for own posts"
                    }
                };

                await SeedModulePermissionsAsync(context, "BlogAuthor", authorPermissions);
            }

            // Define blog-specific permissions for regular User role
            var userPermissions = new[]
            {
                new RolePermission
                {
                    Url = "/Blog",
                    HttpMethod = "GET",
                    Description = "View public blog posts"
                },
                new RolePermission
                {
                    Url = "/Blog/*",
                    HttpMethod = "GET",
                    Description = "View individual blog post details"
                },
                new RolePermission
                {
                    Url = "/Blog/MyPosts",
                    HttpMethod = "GET",
                    Description = "View own posts if user creates content"
                },
                new RolePermission
                {
                    Url = "/Blog/*/comment",
                    HttpMethod = "POST",
                    Description = "Post comments on blog posts"
                },
                new RolePermission
                {
                    Url = "/Category",
                    HttpMethod = "GET",
                    Description = "View categories"
                },
                new RolePermission
                {
                    Url = "/Tag",
                    HttpMethod = "GET",
                    Description = "View tags"
                }
            };

            await SeedModulePermissionsAsync(context, "User", userPermissions);
        }

        private async Task SeedBlogSettingsAsync(AppDbContext context)
        {
            // Check if blog settings exist
            var settingsExist = await context.Set<Setting>()
                .AnyAsync(s => s.Key.StartsWith("Blog."));

            if (!settingsExist)
            {
                var blogSettings = new[]
                {
                    // General Blog Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.Title",
                        Value = "MRCMS Blog",
                        Description = "The title of the blog",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.Description",
                        Value = "A modern content management system blog",
                        Description = "The description of the blog",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.PostsPerPage",
                        Value = "10",
                        Description = "Number of posts to display per page",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.DefaultCategory",
                        Value = "General",
                        Description = "Default category for new posts",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Comment Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableComments",
                        Value = "true",
                        Description = "Enable comments on blog posts",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.ModerateComments",
                        Value = "true",
                        Description = "Require comment moderation before publishing",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.AllowGuestComments",
                        Value = "false",
                        Description = "Allow non-authenticated users to comment",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableCommentAttachments",
                        Value = "true",
                        Description = "Allow file attachments in comments",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.CommentMaxFileSize",
                        Value = "10485760",
                        Description = "Maximum file size for comment attachments in bytes (10MB)",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },

                    // SEO Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableSEO",
                        Value = "true",
                        Description = "Enable SEO features",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.MetaKeywords",
                        Value = "blog, cms, content management",
                        Description = "Default meta keywords for blog pages",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.MetaDescription",
                        Value = "A modern blog powered by MRCMS",
                        Description = "Default meta description for blog pages",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Feed Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableRSS",
                        Value = "true",
                        Description = "Enable RSS feed for blog posts",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.RSSItemsCount",
                        Value = "25",
                        Description = "Number of items to include in RSS feed",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Media Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableFeaturedImages",
                        Value = "true",
                        Description = "Enable featured images for blog posts",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.ImageMaxSize",
                        Value = "5242880",
                        Description = "Maximum image file size in bytes (5MB)",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableImageOptimization",
                        Value = "true",
                        Description = "Enable automatic image optimization",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Analytics Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.TrackViews",
                        Value = "true",
                        Description = "Track blog post view counts",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.GoogleAnalyticsId",
                        Value = "",
                        Description = "Google Analytics tracking ID",
                        Category = "Blog",
                        IsPublic = false,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Social Media Settings
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.EnableSocialSharing",
                        Value = "true",
                        Description = "Enable social media sharing buttons",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.TwitterHandle",
                        Value = "",
                        Description = "Twitter handle for social media integration",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Setting
                    {
                        Id = Guid.NewGuid(),
                        Key = "Blog.FacebookPageUrl",
                        Value = "",
                        Description = "Facebook page URL for social media integration",
                        Category = "Blog",
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Set<Setting>().AddRange(blogSettings);
                await context.SaveChangesAsync();
                
                Logger?.LogInformation($"Seeded {blogSettings.Length} blog settings");
            }
        }
    }
}