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
                Content = @"<p>ASP.NET Core MVC is a powerful framework for building web applications. In this comprehensive guide, we'll explore the fundamental concepts and best practices.</p>
                           <h2>What is ASP.NET Core MVC?</h2>
                           <p>ASP.NET Core MVC provides a patterns-based way to build dynamic websites that enables a clean separation of concerns.</p>
                           <h2>Key Features</h2>
                           <ul>
                               <li>Cross-platform support</li>
                               <li>High performance</li>
                               <li>Built-in dependency injection</li>
                               <li>Modular framework</li>
                           </ul>",
                Summary = "Learn the basics of ASP.NET Core MVC framework and start building modern web applications.",
                FeaturedImageUrl = "/images/blog/aspnet-core-mvc.jpg",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                ViewCount = 245,
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
                           <p>Always use async methods when querying the database to improve application scalability.</p>
                           <h2>2. Optimize Your Queries</h2>
                           <p>Use projection to select only the fields you need, reducing data transfer and improving performance.</p>",
                Summary = "Discover the best practices for using Entity Framework Core effectively in your applications.",
                FeaturedImageUrl = "/images/blog/ef-core-best-practices.jpg",
                CategoryId = techCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                ViewCount = 532,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-20),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new BlogPost
            {
                Title = "Building a Customer Support System",
                Slug = "building-customer-support-system",
                Content = @"<p>A robust customer support system is essential for any business. Let's explore how to build one from scratch.</p>
                           <h2>Core Components</h2>
                           <p>Every support system needs ticket management, user authentication, and reporting capabilities.</p>
                           <h2>Implementation Strategy</h2>
                           <p>Start with a solid architecture and gradually add features based on business requirements.</p>",
                Summary = "Step-by-step guide to building a comprehensive customer support system.",
                FeaturedImageUrl = "/images/blog/support-system.jpg",
                CategoryId = businessCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                ViewCount = 189,
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
                           <p>DI makes your code more testable, maintainable, and flexible.</p>",
                Summary = "Master the concepts of dependency injection in .NET applications.",
                FeaturedImageUrl = "/images/blog/dependency-injection.jpg",
                CategoryId = tutorialCategory?.Id ?? 1,
                AuthorId = blogAuthor?.Id ?? adminUser.Id,
                ViewCount = 412,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new BlogPost
            {
                Title = "Draft: Microservices Architecture Guide",
                Slug = "microservices-architecture-guide",
                Content = @"<p>This is a draft post about microservices architecture...</p>",
                Summary = "Comprehensive guide to microservices architecture (Draft)",
                CategoryId = techCategory?.Id ?? 1,
                AuthorId = adminUser.Id,
                ViewCount = 0,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
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