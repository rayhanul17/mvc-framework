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
        
        // Seed comprehensive test data
        await SeedTestTeamMembersAsync(userManager);
        await SeedBlogPostsAsync(context, userManager);
        await SeedTicketsAsync(context, userManager);
        await SeedAuditLogsAsync(context, userManager);
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

        // Seed Customer Support Team Members
        await SeedSupportTeamAsync(userManager);
        
        // Seed Test Customers (testuser1 to testuser5)
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
                
                if (result.Succeeded)
                {
                    // Add CustomerSupportCustomer role to test users
                    await userManager.AddToRoleAsync(testUser, "CustomerSupportCustomer");
                }
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
                },
                new Menu
                {
                    Name = "Configuration",
                    DisplayName = "Configuration",
                    Area = "CustomerSupport",
                    Controller = "Configuration",
                    Action = "Index",
                    Icon = "fas fa-cog",
                    ParentId = supportMenu.Id,
                    Order = 4,
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

    private static async Task SeedSupportTeamAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed Customer Support Admin
        var supportAdminEmail = "support.admin@example.com";
        if (await userManager.FindByEmailAsync(supportAdminEmail) == null)
        {
            var supportAdmin = new ApplicationUser
            {
                UserName = supportAdminEmail,
                Email = supportAdminEmail,
                FullName = "Sarah Johnson",
                Description = "Customer Support Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };
            
            var result = await userManager.CreateAsync(supportAdmin, "Support@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supportAdmin, "CustomerSupportAdmin");
            }
        }

        // Seed Customer Support Manager
        var supportManagerEmail = "support.manager@example.com";
        if (await userManager.FindByEmailAsync(supportManagerEmail) == null)
        {
            var supportManager = new ApplicationUser
            {
                UserName = supportManagerEmail,
                Email = supportManagerEmail,
                FullName = "Michael Davis",
                Description = "Customer Support Manager",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };
            
            var result = await userManager.CreateAsync(supportManager, "Manager@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supportManager, "CustomerSupportManager");
            }
        }

        // Seed Customer Support Agents
        string[] agentNames = { "Emily Wilson", "James Brown", "Lisa Martinez", "Robert Taylor" };
        for (int i = 0; i < agentNames.Length; i++)
        {
            var agentEmail = $"agent{i + 1}@example.com";
            if (await userManager.FindByEmailAsync(agentEmail) == null)
            {
                var agent = new ApplicationUser
                {
                    UserName = agentEmail,
                    Email = agentEmail,
                    FullName = agentNames[i],
                    Description = "Customer Support Agent",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsSuperAdmin = false
                };
                
                var result = await userManager.CreateAsync(agent, "Agent@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agent, "CustomerSupportAgent");
                }
            }
        }
    }

    private static async Task SeedTestTeamMembersAsync(UserManager<ApplicationUser> userManager)
    {
        // Additional team members for testing various scenarios
        var blogAuthorEmail = "blog.author@example.com";
        if (await userManager.FindByEmailAsync(blogAuthorEmail) == null)
        {
            var blogAuthor = new ApplicationUser
            {
                UserName = blogAuthorEmail,
                Email = blogAuthorEmail,
                FullName = "John Writer",
                Description = "Content Creator and Blog Author",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };
            
            var result = await userManager.CreateAsync(blogAuthor, "BlogAuthor@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(blogAuthor, "Administrator");
            }
        }
    }

    private static async Task SeedBlogPostsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.BlogPosts.AnyAsync())
            return;

        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        var blogAuthor = await userManager.FindByEmailAsync("blog.author@example.com");
        
        if (adminUser == null)
            return;

        var techCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "technology");
        var businessCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "business");
        var tutorialCategory = await context.BlogCategories.FirstOrDefaultAsync(c => c.Slug == "tutorial");

        var blogPosts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Getting Started with ASP.NET Core MVC",
                Slug = "getting-started-aspnet-core-mvc",
                Content = @"<p>ASP.NET Core MVC is a powerful, cross-platform framework for building modern web applications. In this comprehensive guide, we'll explore the fundamental concepts, architecture patterns, and best practices that make ASP.NET Core MVC the go-to choice for enterprise web development.</p>
                           
                           <h2>What is ASP.NET Core MVC?</h2>
                           <p>ASP.NET Core MVC is a rich framework for building web apps and APIs using the Model-View-Controller design pattern. It provides a patterns-based way to build dynamic websites that enables a clean separation of concerns, giving you full control over markup while supporting test-driven development and using the latest web standards.</p>
                           
                           <div class=""alert alert-info"">
                               <strong>Did you know?</strong> ASP.NET Core MVC is built on top of ASP.NET Core runtime, which makes it incredibly fast and lightweight compared to traditional .NET Framework applications.
                           </div>
                           
                           <h2>Key Features and Benefits</h2>
                           <ul>
                               <li><strong>Cross-platform support:</strong> Runs on Windows, macOS, and Linux</li>
                               <li><strong>High performance:</strong> Optimized for speed and scalability</li>
                               <li><strong>Built-in dependency injection:</strong> Clean, testable code architecture</li>
                               <li><strong>Modular framework:</strong> Include only what you need</li>
                               <li><strong>Cloud-ready:</strong> Built for modern cloud deployment scenarios</li>
                               <li><strong>Open source:</strong> Transparent development and community contributions</li>
                           </ul>
                           
                           <h2>MVC Architecture Pattern</h2>
                           <p>The Model-View-Controller (MVC) architectural pattern separates an application into three main logical components:</p>
                           <table class=""table table-striped"">
                               <thead>
                                   <tr>
                                       <th>Component</th>
                                       <th>Responsibility</th>
                                       <th>Example</th>
                                   </tr>
                               </thead>
                               <tbody>
                                   <tr>
                                       <td><strong>Model</strong></td>
                                       <td>Represents data and business logic</td>
                                       <td>User, Product, Order entities</td>
                                   </tr>
                                   <tr>
                                       <td><strong>View</strong></td>
                                       <td>Handles the display logic and user interface</td>
                                       <td>Razor pages, HTML templates</td>
                                   </tr>
                                   <tr>
                                       <td><strong>Controller</strong></td>
                                       <td>Handles user input and coordinates Model and View</td>
                                       <td>HomeController, AccountController</td>
                                   </tr>
                               </tbody>
                           </table>
                           
                           <h2>Getting Started - Your First Application</h2>
                           <p>To start building your first ASP.NET Core MVC application, you'll need:</p>
                           <ol>
                               <li><strong>.NET SDK:</strong> Download from <a href=""https://dotnet.microsoft.com/download"" target=""_blank"">dotnet.microsoft.com</a></li>
                               <li><strong>IDE:</strong> Visual Studio, Visual Studio Code, or JetBrains Rider</li>
                               <li><strong>Basic C# knowledge:</strong> Understanding of object-oriented programming</li>
                           </ol>
                           
                           <h3>Creating Your Project</h3>
                           <pre><code>dotnet new mvc -n MyFirstMvcApp
cd MyFirstMvcApp
dotnet run</code></pre>
                           
                           <p>This creates a new MVC project with a basic structure including controllers, views, and models. The application will be available at <code>https://localhost:5001</code> by default.</p>
                           
                           <h2>Best Practices for Success</h2>
                           <blockquote class=""blockquote"">
                               <p>""The key to successful ASP.NET Core MVC development lies in understanding the separation of concerns and leveraging the framework's built-in features effectively.""</p>
                               <footer class=""blockquote-footer"">Industry Expert</footer>
                           </blockquote>
                           
                           <ul>
                               <li>Keep controllers thin - business logic belongs in services</li>
                               <li>Use dependency injection for better testability</li>
                               <li>Implement proper error handling and logging</li>
                               <li>Follow RESTful conventions for your routes</li>
                               <li>Use model validation attributes</li>
                               <li>Implement security best practices from day one</li>
                           </ul>
                           
                           <h2>Next Steps</h2>
                           <p>Once you have your basic MVC application running, consider exploring these advanced topics:</p>
                           <div class=""row"">
                               <div class=""col-md-6"">
                                   <h4>Backend Topics</h4>
                                   <ul>
                                       <li>Entity Framework Core</li>
                                       <li>Authentication & Authorization</li>
                                       <li>Web APIs and RESTful services</li>
                                       <li>Middleware pipeline</li>
                                   </ul>
                               </div>
                               <div class=""col-md-6"">
                                   <h4>Frontend Topics</h4>
                                   <ul>
                                       <li>Razor Pages and Views</li>
                                       <li>Client-side libraries integration</li>
                                       <li>Responsive design with Bootstrap</li>
                                       <li>JavaScript and AJAX</li>
                                   </ul>
                               </div>
                           </div>",
                Summary = "Learn the basics of ASP.NET Core MVC framework and start building modern web applications.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800&h=400&fit=crop",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                Tags = "ASP.NET Core,MVC,Web Development,C#,Tutorial",
                MetaTitle = "Getting Started with ASP.NET Core MVC - Complete Guide",
                MetaDescription = "Learn ASP.NET Core MVC from scratch with this comprehensive tutorial. Build modern web applications with best practices.",
                MetaKeywords = "ASP.NET Core, MVC, Tutorial, Web Development",
                ViewCount = 2456,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new BlogPost
            {
                Title = "10 Best Practices for Entity Framework Core",
                Slug = "best-practices-entity-framework-core",
                Content = @"<p>Entity Framework Core is a modern object-database mapper for .NET. Here are the top 10 best practices to follow.</p>
                           <h2>1. Use Async Methods</h2>
                           <p>Always use async methods when querying the database to improve application scalability. This prevents blocking threads and improves overall application throughput.</p>
                           <h2>2. Optimize Your Queries</h2>
                           <p>Use projection to select only the fields you need, reducing data transfer and improving performance. Avoid the N+1 query problem by using Include() for eager loading.</p>
                           <h2>3. Use No-Tracking Queries for Read-Only Operations</h2>
                           <p>When you're only reading data and not planning to update it, use AsNoTracking() to improve performance.</p>
                           <h2>4. Batch Your Operations</h2>
                           <p>Instead of saving changes after each operation, batch multiple operations and call SaveChanges() once.</p>",
                Summary = "Discover the best practices for using Entity Framework Core effectively in your applications.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=800&h=400&fit=crop",
                CategoryId = techCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                Tags = "Entity Framework,EF Core,Database,ORM,Performance,Best Practices",
                MetaTitle = "10 Best Practices for Entity Framework Core - Expert Guide",
                MetaDescription = "Master Entity Framework Core with these 10 essential best practices for better performance and maintainability.",
                MetaKeywords = "Entity Framework Core, EF Core, Best Practices, Performance",
                ViewCount = 5324,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-20),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new BlogPost
            {
                Title = "Building a Scalable Microservices Architecture",
                Slug = "building-scalable-microservices-architecture",
                Content = @"<p>Microservices architecture has become the go-to solution for building scalable, maintainable applications. Let's explore how to design and implement a robust microservices system.</p>
                           <h2>Understanding Microservices</h2>
                           <p>Microservices are small, independent services that work together to form a complete application. Each service is responsible for a specific business capability.</p>
                           <h2>Key Principles</h2>
                           <ul>
                               <li>Single Responsibility: Each service should do one thing well</li>
                               <li>Autonomous: Services should be independently deployable</li>
                               <li>Decentralized: Avoid shared databases and centralized governance</li>
                               <li>Failure Isolation: One service failure shouldn't bring down the entire system</li>
                           </ul>
                           <h2>Communication Patterns</h2>
                           <p>Services can communicate through REST APIs, message queues, or event streaming platforms like Kafka.</p>",
                Summary = "Learn how to design and build scalable microservices architecture with practical examples.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&h=400&fit=crop",
                CategoryId = techCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                Tags = "Microservices,Architecture,Cloud,Docker,Kubernetes,DevOps",
                MetaTitle = "Building Scalable Microservices Architecture - Complete Guide",
                MetaDescription = "Learn how to build scalable microservices architecture with best practices and real-world examples.",
                MetaKeywords = "Microservices, Architecture, Scalability, Cloud Native",
                ViewCount = 3891,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-25),
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new BlogPost
            {
                Title = "Modern Authentication with JWT in ASP.NET Core",
                Slug = "modern-authentication-jwt-aspnet-core",
                Content = @"<p>JSON Web Tokens (JWT) have become the standard for modern API authentication. Let's implement secure JWT authentication in ASP.NET Core.</p>
                           <h2>What is JWT?</h2>
                           <p>JWT is an open standard (RFC 7519) that defines a compact way for securely transmitting information between parties as a JSON object.</p>
                           <h2>JWT Structure</h2>
                           <p>A JWT consists of three parts: Header, Payload, and Signature, separated by dots (xxxxx.yyyyy.zzzzz).</p>
                           <h2>Implementation Steps</h2>
                           <ol>
                               <li>Install required NuGet packages</li>
                               <li>Configure JWT in Startup.cs</li>
                               <li>Create token generation service</li>
                               <li>Implement login endpoint</li>
                               <li>Protect API endpoints with [Authorize] attribute</li>
                           </ol>",
                Summary = "Implement secure JWT authentication in your ASP.NET Core applications with this comprehensive guide.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1555949963-ff9fe0c870eb?w=800&h=400&fit=crop",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                Tags = "JWT,Authentication,Security,ASP.NET Core,API",
                MetaTitle = "JWT Authentication in ASP.NET Core - Complete Implementation Guide",
                MetaDescription = "Learn how to implement secure JWT authentication in ASP.NET Core with step-by-step instructions.",
                MetaKeywords = "JWT, Authentication, ASP.NET Core, Security",
                ViewCount = 4123,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-18),
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new BlogPost
            {
                Title = "Building a Customer Support System",
                Slug = "building-customer-support-system",
                Content = @"<p>A robust customer support system is essential for any business. Let's explore how to build one from scratch.</p>
                           <h2>Core Components</h2>
                           <p>Every support system needs ticket management, user authentication, and reporting capabilities.</p>
                           <h2>Ticket Management Features</h2>
                           <ul>
                               <li>Ticket creation and tracking</li>
                               <li>Priority and category assignment</li>
                               <li>Agent assignment and escalation</li>
                               <li>Customer communication history</li>
                               <li>SLA tracking and alerts</li>
                           </ul>
                           <h2>Implementation Strategy</h2>
                           <p>Start with a solid architecture and gradually add features based on business requirements. Use a modular approach to ensure scalability.</p>",
                Summary = "Step-by-step guide to building a comprehensive customer support system.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=800&h=400&fit=crop",
                CategoryId = businessCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                Tags = "Customer Support,Business,SaaS,Ticketing System",
                MetaTitle = "Building a Customer Support System - Complete Guide",
                MetaDescription = "Learn how to build a comprehensive customer support system with ticketing, reporting, and communication features.",
                MetaKeywords = "Customer Support, Ticketing System, Business Software",
                ViewCount = 1897,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-15),
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new BlogPost
            {
                Title = "Understanding Dependency Injection in .NET",
                Slug = "understanding-dependency-injection-dotnet",
                Content = @"<p>Dependency Injection (DI) is a design pattern that helps create loosely coupled applications.</p>
                           <h2>Why Use Dependency Injection?</h2>
                           <p>DI makes your code more testable, maintainable, and flexible. It's a fundamental principle of SOLID design.</p>
                           <h2>Types of Dependency Injection</h2>
                           <ul>
                               <li>Constructor Injection (most common and recommended)</li>
                               <li>Property Injection</li>
                               <li>Method Injection</li>
                           </ul>
                           <h2>Service Lifetimes in .NET Core</h2>
                           <p>Understanding service lifetimes is crucial: Transient, Scoped, and Singleton. Each has its use cases and implications.</p>",
                Summary = "Master the concepts of dependency injection in .NET applications.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1517180102446-f3fb51b5f9f9?w=800&h=400&fit=crop",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                Tags = "Dependency Injection,DI,Design Patterns,.NET,SOLID",
                MetaTitle = "Understanding Dependency Injection in .NET - Complete Guide",
                MetaDescription = "Master dependency injection in .NET with practical examples and best practices.",
                MetaKeywords = "Dependency Injection, DI, .NET, Design Patterns",
                ViewCount = 4127,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new BlogPost
            {
                Title = "Cloud Migration Strategies for Enterprise Applications",
                Slug = "cloud-migration-strategies-enterprise",
                Content = @"<p>Moving enterprise applications to the cloud requires careful planning and execution. Let's explore proven migration strategies.</p>
                           <h2>The 6 R's of Cloud Migration</h2>
                           <ul>
                               <li>Rehost (Lift and Shift)</li>
                               <li>Replatform (Lift, Tinker, and Shift)</li>
                               <li>Repurchase (Drop and Shop)</li>
                               <li>Refactor/Re-architect</li>
                               <li>Retire</li>
                               <li>Retain</li>
                           </ul>
                           <h2>Choosing the Right Strategy</h2>
                           <p>Consider factors like business goals, technical debt, compliance requirements, and available resources when selecting your migration approach.</p>",
                Summary = "Comprehensive guide to cloud migration strategies for enterprise applications.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&h=400&fit=crop",
                CategoryId = businessCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                Tags = "Cloud Migration,AWS,Azure,Enterprise,DevOps",
                MetaTitle = "Cloud Migration Strategies for Enterprise Applications",
                MetaDescription = "Learn proven cloud migration strategies for moving enterprise applications to AWS, Azure, or Google Cloud.",
                MetaKeywords = "Cloud Migration, Enterprise, AWS, Azure",
                ViewCount = 2341,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-8),
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new BlogPost
            {
                Title = "Real-time Applications with SignalR in ASP.NET Core",
                Slug = "realtime-applications-signalr-aspnet-core",
                Content = @"<p>SignalR makes it incredibly easy to add real-time web functionality to your applications. Let's build a real-time chat application.</p>
                           <h2>What is SignalR?</h2>
                           <p>SignalR is a library that simplifies adding real-time web functionality to apps. Real-time web functionality enables server-side code to push content to clients instantly.</p>
                           <h2>Use Cases</h2>
                           <ul>
                               <li>Chat applications</li>
                               <li>Real-time dashboards</li>
                               <li>Collaborative editing</li>
                               <li>Live notifications</li>
                               <li>Gaming applications</li>
                           </ul>",
                Summary = "Build real-time applications with SignalR in ASP.NET Core.",
                FeaturedImageUrl = "https://images.unsplash.com/photo-1611746872915-64382b5c76da?w=800&h=400&fit=crop",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                Tags = "SignalR,Real-time,WebSockets,ASP.NET Core,Chat",
                MetaTitle = "Real-time Applications with SignalR in ASP.NET Core",
                MetaDescription = "Learn how to build real-time applications using SignalR in ASP.NET Core with practical examples.",
                MetaKeywords = "SignalR, Real-time, WebSockets, ASP.NET Core",
                ViewCount = 3567,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new BlogPost
            {
                Title = "Draft: Advanced Caching Strategies in .NET",
                Slug = "advanced-caching-strategies-dotnet",
                Content = @"<p>This is a draft post about advanced caching strategies in .NET applications...</p>",
                Summary = "Explore advanced caching strategies to improve application performance (Draft)",
                CategoryId = techCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                Tags = "Caching,Performance,Redis,.NET",
                ViewCount = 0,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.BlogPosts.AddRangeAsync(blogPosts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTicketsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Tickets.AnyAsync())
            return;

        // Get users for ticket creation
        var customer1 = await userManager.FindByEmailAsync("testuser1@example.com");
        var customer2 = await userManager.FindByEmailAsync("testuser2@example.com");
        var customer3 = await userManager.FindByEmailAsync("testuser3@example.com");
        var agent1 = await userManager.FindByEmailAsync("agent1@example.com");
        var agent2 = await userManager.FindByEmailAsync("agent2@example.com");
        var manager = await userManager.FindByEmailAsync("support.manager@example.com");

        if (customer1 == null || agent1 == null)
            return;

        var tickets = new List<Ticket>
        {
            new Ticket
            {
                TicketNumber = "TKT-2024-001",
                Title = "Cannot login to my account",
                Description = "I'm unable to login to my account. I've tried resetting my password but still getting an error message.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.High,
                Category = "Account",
                CustomerId = customer1.Id,
                AssignedToId = agent1?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-002",
                Title = "Billing issue - duplicate charge",
                Description = "I was charged twice for my subscription this month. Please refund the duplicate charge.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Critical,
                Category = "Billing",
                CustomerId = customer2?.Id ?? customer1.Id,
                AssignedToId = agent2?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-003",
                Title = "Feature request - Export to PDF",
                Description = "It would be great if we could export reports to PDF format. This would help with sharing reports with clients.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Low,
                Category = "Feature",
                CustomerId = customer3?.Id ?? customer1.Id,
                AssignedToId = null, // Unassigned
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-004",
                Title = "Application crashes on startup",
                Description = "The application crashes immediately after launching. Error message: 'Unable to load configuration file'",
                Status = TicketStatus.Resolved,
                Priority = TicketPriority.High,
                Category = "Technical",
                CustomerId = customer1.Id,
                AssignedToId = agent1?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-8),
                ResolvedAt = DateTime.UtcNow.AddDays(-8),
                ResolutionNotes = "Configuration file was corrupted. Provided new configuration file to customer."
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-005",
                Title = "Slow performance issues",
                Description = "The system has been running very slowly for the past week. Page load times are over 30 seconds.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Medium,
                Category = "Technical",
                CustomerId = customer2?.Id ?? customer1.Id,
                AssignedToId = agent2?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-006",
                Title = "Request for training materials",
                Description = "We have new team members joining. Could you provide training materials and documentation?",
                Status = TicketStatus.Closed,
                Priority = TicketPriority.Low,
                Category = "Other",
                CustomerId = customer3?.Id ?? customer1.Id,
                AssignedToId = agent1?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-12),
                ResolvedAt = DateTime.UtcNow.AddDays(-12),
                ClosedAt = DateTime.UtcNow.AddDays(-11),
                ResolutionNotes = "Provided comprehensive training materials and scheduled training session."
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-007",
                Title = "Integration with third-party API failing",
                Description = "Our integration with the payment gateway API is returning 401 errors since yesterday.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Critical,
                Category = "Technical",
                CustomerId = customer1.Id,
                AssignedToId = null, // Unassigned critical ticket
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new Ticket
            {
                TicketNumber = "TKT-2024-008",
                Title = "Update payment method",
                Description = "I need to update my payment method from credit card to bank transfer.",
                Status = TicketStatus.OnHold,
                Priority = TicketPriority.Medium,
                Category = "Billing",
                CustomerId = customer2?.Id ?? customer1.Id,
                AssignedToId = agent2?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Tickets.AddRangeAsync(tickets);
        await context.SaveChangesAsync();

        // Add ticket comments
        await SeedTicketCommentsAsync(context, tickets, agent1, customer1);
        
        // Add ticket history
        await SeedTicketHistoryAsync(context, tickets, agent1);
    }

    private static async Task SeedTicketCommentsAsync(ApplicationDbContext context, List<Ticket> tickets, ApplicationUser? agent, ApplicationUser customer)
    {
        if (await context.TicketComments.AnyAsync())
            return;

        var resolvedTicket = tickets.FirstOrDefault(t => t.Status == TicketStatus.Resolved);
        if (resolvedTicket != null && agent != null)
        {
            var comments = new List<TicketComment>
            {
                new TicketComment
                {
                    TicketId = resolvedTicket.Id,
                    UserId = customer.Id,
                    Comment = "I've attached a screenshot of the error message.",
                    IsInternal = false,
                    CreatedAt = resolvedTicket.CreatedAt.AddHours(1)
                },
                new TicketComment
                {
                    TicketId = resolvedTicket.Id,
                    UserId = agent.Id,
                    Comment = "Thank you for the screenshot. I can see the issue now. Let me investigate this.",
                    IsInternal = false,
                    CreatedAt = resolvedTicket.CreatedAt.AddHours(2)
                },
                new TicketComment
                {
                    TicketId = resolvedTicket.Id,
                    UserId = agent.Id,
                    Comment = "Internal note: Configuration file path issue. Need to provide updated config.",
                    IsInternal = true,
                    CreatedAt = resolvedTicket.CreatedAt.AddHours(3)
                },
                new TicketComment
                {
                    TicketId = resolvedTicket.Id,
                    UserId = agent.Id,
                    Comment = "I've sent you a new configuration file via email. Please replace the existing one and try again.",
                    IsInternal = false,
                    CreatedAt = resolvedTicket.CreatedAt.AddHours(4)
                },
                new TicketComment
                {
                    TicketId = resolvedTicket.Id,
                    UserId = customer.Id,
                    Comment = "That worked! Thank you so much for the quick resolution.",
                    IsInternal = false,
                    CreatedAt = resolvedTicket.CreatedAt.AddHours(5)
                }
            };

            await context.TicketComments.AddRangeAsync(comments);
        }

        var inProgressTicket = tickets.FirstOrDefault(t => t.Status == TicketStatus.InProgress);
        if (inProgressTicket != null && agent != null)
        {
            var comments = new List<TicketComment>
            {
                new TicketComment
                {
                    TicketId = inProgressTicket.Id,
                    UserId = agent?.Id ?? customer.Id,
                    Comment = "I'm currently investigating this issue. I'll need to review your account details.",
                    IsInternal = false,
                    CreatedAt = inProgressTicket.CreatedAt.AddHours(2)
                },
                new TicketComment
                {
                    TicketId = inProgressTicket.Id,
                    UserId = customer.Id,
                    Comment = "Please let me know if you need any additional information from my end.",
                    IsInternal = false,
                    CreatedAt = inProgressTicket.CreatedAt.AddHours(3)
                }
            };

            await context.TicketComments.AddRangeAsync(comments);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTicketHistoryAsync(ApplicationDbContext context, List<Ticket> tickets, ApplicationUser? agent)
    {
        if (await context.TicketHistories.AnyAsync())
            return;

        var histories = new List<TicketHistory>();

        foreach (var ticket in tickets.Where(t => t.AssignedToId != null))
        {
            histories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                Action = "Ticket Created",
                UserId = ticket.CustomerId,
                Description = $"Ticket {ticket.TicketNumber} was created",
                CreatedAt = ticket.CreatedAt
            });

            if (ticket.AssignedToId != null)
            {
                histories.Add(new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "Ticket Assigned",
                    UserId = agent?.Id ?? ticket.CustomerId,
                    Description = $"Ticket assigned to {agent?.FullName ?? "Support Agent"}",
                    OldValue = "Unassigned",
                    NewValue = agent?.FullName ?? "Support Agent",
                    CreatedAt = ticket.CreatedAt.AddMinutes(30)
                });
            }

            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            {
                histories.Add(new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "Status Changed",
                    UserId = agent?.Id ?? ticket.CustomerId,
                    Description = "Ticket status changed to Resolved",
                    OldValue = "InProgress",
                    NewValue = "Resolved",
                    CreatedAt = ticket.ResolvedAt ?? ticket.UpdatedAt ?? DateTime.UtcNow
                });
            }

            if (ticket.Status == TicketStatus.Closed)
            {
                histories.Add(new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "Ticket Closed",
                    UserId = ticket.CustomerId,
                    Description = "Ticket was closed",
                    CreatedAt = ticket.ClosedAt ?? ticket.UpdatedAt ?? DateTime.UtcNow
                });
            }
        }

        if (histories.Any())
        {
            await context.TicketHistories.AddRangeAsync(histories);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task SeedAuditLogsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Check if logs already exist
        if (await context.Logs.AnyAsync())
            return;

        // Get users for audit log entries
        var superAdmin = await userManager.FindByEmailAsync("superadmin@example.com");
        var admin = await userManager.FindByEmailAsync("admin@example.com");
        var agent = await userManager.FindByEmailAsync("agent1@example.com");
        var manager = await userManager.FindByEmailAsync("manager1@example.com");

        var logs = new List<Log>
        {
            // User management logs
            new Log
            {
                TableName = "Users",
                EntityId = 1,
                Action = "Added",
                Changes = "New user created: agent1@example.com",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Log
            {
                TableName = "Users",
                EntityId = 2,
                Action = "Modified",
                Changes = "User role updated from Agent to Manager",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Log
            {
                TableName = "Users",
                EntityId = 3,
                Action = "Modified",
                Changes = "Password reset requested",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-20)
            },
            
            // Role management logs
            new Log
            {
                TableName = "Roles",
                EntityId = 1,
                Action = "Added",
                Changes = "New role created: CustomerSupportAgent",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Log
            {
                TableName = "Roles",
                EntityId = 2,
                Action = "Modified",
                Changes = "Role permissions updated",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-22)
            },
            
            // Ticket management logs
            new Log
            {
                TableName = "Tickets",
                EntityId = 1,
                Action = "Added",
                Changes = "New ticket created: TKT-2024-001",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 1,
                Action = "Modified",
                Changes = "Status changed from Open to InProgress",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-14)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 2,
                Action = "Added",
                Changes = "New ticket created: TKT-2024-002",
                UserId = manager?.Id,
                IpAddress = "192.168.1.103",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 2,
                Action = "Modified",
                Changes = "Priority changed from Low to High",
                UserId = manager?.Id,
                IpAddress = "192.168.1.103",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-11)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 1,
                Action = "Modified",
                Changes = "Status changed from InProgress to Closed",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-10)
            },
            
            // Blog management logs
            new Log
            {
                TableName = "BlogPosts",
                EntityId = 1,
                Action = "Added",
                Changes = "New blog post published: Getting Started with Our Platform",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Log
            {
                TableName = "BlogPosts",
                EntityId = 1,
                Action = "Modified",
                Changes = "Blog post updated: Added new section",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Log
            {
                TableName = "BlogCategories",
                EntityId = 1,
                Action = "Added",
                Changes = "New category created: Technology",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-9)
            },
            
            // Site settings logs
            new Log
            {
                TableName = "SiteSettings",
                EntityId = 1,
                Action = "Modified",
                Changes = "Site title updated",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Log
            {
                TableName = "SiteSettings",
                EntityId = 2,
                Action = "Modified",
                Changes = "Footer text updated",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-4)
            },
            
            // Menu management logs
            new Log
            {
                TableName = "Menus",
                EntityId = 1,
                Action = "Added",
                Changes = "New menu item created: Dashboard",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Log
            {
                TableName = "Menus",
                EntityId = 2,
                Action = "Modified",
                Changes = "Menu order changed",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddDays(-16)
            },
            
            // Recent activity logs
            new Log
            {
                TableName = "Users",
                EntityId = 4,
                Action = "Modified",
                Changes = "User account activated",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 3,
                Action = "Added",
                Changes = "New ticket created: TKT-2024-003",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddHours(-6)
            },
            new Log
            {
                TableName = "Tickets",
                EntityId = 3,
                Action = "Modified",
                Changes = "Ticket assigned to agent",
                UserId = manager?.Id,
                IpAddress = "192.168.1.103",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Log
            {
                TableName = "Users",
                EntityId = 5,
                Action = "Deleted",
                Changes = "User account deleted: testuser@example.com",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                LoggedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        await context.Logs.AddRangeAsync(logs);
        
        // Add some archived logs
        var archivedLogs = new List<LogArchive>
        {
            new LogArchive
            {
                TableName = "Users",
                EntityId = 10,
                Action = "Added",
                Changes = "Old user created",
                UserId = superAdmin?.Id,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0",
                LoggedAt = DateTime.UtcNow.AddDays(-60),
                ArchivedAt = DateTime.UtcNow.AddDays(-30)
            },
            new LogArchive
            {
                TableName = "Tickets",
                EntityId = 100,
                Action = "Modified",
                Changes = "Old ticket updated",
                UserId = agent?.Id,
                IpAddress = "192.168.1.102",
                UserAgent = "Mozilla/5.0",
                LoggedAt = DateTime.UtcNow.AddDays(-90),
                ArchivedAt = DateTime.UtcNow.AddDays(-30)
            },
            new LogArchive
            {
                TableName = "BlogPosts",
                EntityId = 50,
                Action = "Deleted",
                Changes = "Old blog post deleted",
                UserId = admin?.Id,
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0",
                LoggedAt = DateTime.UtcNow.AddDays(-120),
                ArchivedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        await context.LogArchives.AddRangeAsync(archivedLogs);
        await context.SaveChangesAsync();
    }
}