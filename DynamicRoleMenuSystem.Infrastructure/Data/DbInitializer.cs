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
        
        // Seed Customer Support menus specifically
        await SeedCustomerSupportMenusAsync(context, roleManager);
        
        // Ensure SuperAdmin has access to ALL menus
        await EnsureSuperAdminHasAllMenusAsync(context, roleManager);
        
        // Seed blog categories
        await SeedBlogCategoriesAsync(context);
        
        // Seed site settings
        await SeedSiteSettingsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { "SuperAdmin", "Administrator", "Manager", "Editor", "User", "Customer", "Support", "SupportManager" };
        
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
        
        // Seed Customer Support Users
        
        // Support Manager
        var managerEmail = "support.manager@example.com";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                FullName = "Support Manager",
                Description = "Customer Support Manager",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(managerUser, "Manager@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "SupportManager");
            }
        }
        
        // Support Agent 1
        var agent1Email = "support1@example.com";
        var agent1User = await userManager.FindByEmailAsync(agent1Email);
        if (agent1User == null)
        {
            agent1User = new ApplicationUser
            {
                UserName = agent1Email,
                Email = agent1Email,
                FullName = "John Support",
                Description = "Customer Support Agent",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(agent1User, "Support@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agent1User, "Support");
            }
        }
        
        // Support Agent 2
        var agent2Email = "support2@example.com";
        var agent2User = await userManager.FindByEmailAsync(agent2Email);
        if (agent2User == null)
        {
            agent2User = new ApplicationUser
            {
                UserName = agent2Email,
                Email = agent2Email,
                FullName = "Jane Support",
                Description = "Customer Support Agent",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(agent2User, "Support@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agent2User, "Support");
            }
        }
        
        // Sample Customer 1
        var customer1Email = "customer1@example.com";
        var customer1User = await userManager.FindByEmailAsync(customer1Email);
        if (customer1User == null)
        {
            customer1User = new ApplicationUser
            {
                UserName = customer1Email,
                Email = customer1Email,
                FullName = "John Doe",
                Description = "Customer account",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(customer1User, "Customer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer1User, "Customer");
            }
        }
        
        // Sample Customer 2
        var customer2Email = "customer2@example.com";
        var customer2User = await userManager.FindByEmailAsync(customer2Email);
        if (customer2User == null)
        {
            customer2User = new ApplicationUser
            {
                UserName = customer2Email,
                Email = customer2Email,
                FullName = "Jane Smith",
                Description = "Customer account",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(customer2User, "Customer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer2User, "Customer");
            }
        }
        
        // Regular User with support access
        var userEmail = "user@example.com";
        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "Regular User",
                Description = "Regular user with basic support access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(regularUser, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
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
                    },
                    // Customer Support Main Menu
                    new Menu
                    {
                        Name = "Customer Support",
                        DisplayName = "Customer Support",
                        Icon = "fas fa-headset",
                        Order = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Support Dashboard
                    new Menu
                    {
                        Name = "Support Dashboard",
                        DisplayName = "Support Dashboard",
                        Area = "CustomerSupport",
                        Controller = "Dashboard",
                        Action = "Index",
                        Icon = "fas fa-tachometer-alt",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Customer Ticket Management
                    new Menu
                    {
                        Name = "My Tickets",
                        DisplayName = "My Tickets",
                        Area = "CustomerSupport",
                        Controller = "Ticket",
                        Action = "Index",
                        Icon = "fas fa-ticket-alt",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Create New Ticket",
                        DisplayName = "Create New Ticket",
                        Area = "CustomerSupport",
                        Controller = "Ticket",
                        Action = "Create",
                        Icon = "fas fa-plus-circle",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Support Staff Functions
                    new Menu
                    {
                        Name = "Assigned Tickets",
                        DisplayName = "Assigned Tickets",
                        Area = "CustomerSupport",
                        Controller = "SupportTicket",
                        Action = "MyTickets",
                        Icon = "fas fa-user-check",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Support Queue",
                        DisplayName = "Support Queue",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Index",
                        Icon = "fas fa-clipboard-list",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 5,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Pending Review",
                        DisplayName = "Pending Review",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Index",
                        Icon = "fas fa-clock",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 6,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Manager Functions
                    new Menu
                    {
                        Name = "Ticket Management",
                        DisplayName = "Ticket Management",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Index",
                        Icon = "fas fa-tasks",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 7,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Unassigned Tickets",
                        DisplayName = "Unassigned Tickets",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Unassigned",
                        Icon = "fas fa-exclamation-triangle",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 8,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Escalated Tickets",
                        DisplayName = "Escalated Tickets",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Index",
                        Icon = "fas fa-arrow-up",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 9,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Overdue Tickets",
                        DisplayName = "Overdue Tickets",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Index",
                        Icon = "fas fa-hourglass-end",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 10,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Reports & Analytics
                    new Menu
                    {
                        Name = "Reports & Analytics",
                        DisplayName = "Reports & Analytics",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Statistics",
                        Icon = "fas fa-chart-bar",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 11,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Performance Metrics",
                        DisplayName = "Performance Metrics",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Statistics",
                        Icon = "fas fa-chart-line",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 12,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Agent Statistics",
                        DisplayName = "Agent Statistics",
                        Area = "CustomerSupport",
                        Controller = "ManageTicket",
                        Action = "Statistics",
                        Icon = "fas fa-user-chart",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 13,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Notifications
                    new Menu
                    {
                        Name = "Notifications",
                        DisplayName = "Notifications",
                        Area = "CustomerSupport",
                        Controller = "Dashboard",
                        Action = "Notifications",
                        Icon = "fas fa-bell",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 14,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Settings & Configuration
                    new Menu
                    {
                        Name = "Support Settings",
                        DisplayName = "Support Settings",
                        Area = "CustomerSupport",
                        Controller = "Settings",
                        Action = "Index",
                        Icon = "fas fa-cog",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 15,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Ticket Categories",
                        DisplayName = "Ticket Categories",
                        Area = "CustomerSupport",
                        Controller = "Settings",
                        Action = "Categories",
                        Icon = "fas fa-tags",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 16,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "Email Templates",
                        DisplayName = "Email Templates",
                        Area = "CustomerSupport",
                        Controller = "Settings",
                        Action = "EmailTemplates",
                        Icon = "fas fa-envelope",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 17,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "SLA Configuration",
                        DisplayName = "SLA Configuration",
                        Area = "CustomerSupport",
                        Controller = "Settings",
                        Action = "SLA",
                        Icon = "fas fa-stopwatch",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 18,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    
                    // Knowledge Base
                    new Menu
                    {
                        Name = "Knowledge Base",
                        DisplayName = "Knowledge Base",
                        Area = "CustomerSupport",
                        Controller = "KnowledgeBase",
                        Action = "Index",
                        Icon = "fas fa-book",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 19,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Menu
                    {
                        Name = "FAQs",
                        DisplayName = "FAQs",
                        Area = "CustomerSupport",
                        Controller = "KnowledgeBase",
                        Action = "FAQ",
                        Icon = "fas fa-question-circle",
                        ParentId = 10, // Will be updated to correct ID
                        Order = 20,
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
                
                var supportMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Customer Support");
                if (supportMenu != null)
                {
                    var supportChildMenus = menus.Where(m => m.ParentId == 10).ToList();
                    foreach (var childMenu in supportChildMenus)
                    {
                        childMenu.ParentId = supportMenu.Id;
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

                // Assign Customer Support menus to roles
                var customerRole = await roleManager.FindByNameAsync("Customer");
                if (customerRole != null && supportMenu != null)
                {
                    // Customers can view and create their own tickets
                    var customerMenus = await context.Menus
                        .Where(m => m.Name == "Customer Support" || 
                                   m.Name == "My Tickets" || 
                                   m.Name == "Create New Ticket" ||
                                   m.Name == "Notifications" ||
                                   m.Name == "Knowledge Base" ||
                                   m.Name == "FAQs")
                        .ToListAsync();

                    foreach (var menu in customerMenus)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = customerRole.Id,
                            MenuId = menu.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = false,
                            CanDelete = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }

                var supportRole = await roleManager.FindByNameAsync("Support");
                if (supportRole != null && supportMenu != null)
                {
                    // Support staff can work on assigned tickets
                    var supportMenus = await context.Menus
                        .Where(m => m.Name == "Customer Support" || 
                                   m.Name == "Support Dashboard" ||
                                   m.Name == "My Tickets" ||
                                   m.Name == "Assigned Tickets" ||
                                   m.Name == "Support Queue" || 
                                   m.Name == "Pending Review" ||
                                   m.Name == "Notifications" ||
                                   m.Name == "Knowledge Base" ||
                                   m.Name == "FAQs" ||
                                   m.Name == "Agent Statistics")
                        .ToListAsync();

                    foreach (var menu in supportMenus)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = supportRole.Id,
                            MenuId = menu.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = true,
                            CanDelete = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }

                var supportManagerRole = await roleManager.FindByNameAsync("SupportManager");
                if (supportManagerRole != null && supportMenu != null)
                {
                    // Support managers have full access to all support features
                    var managerMenus = await context.Menus
                        .Where(m => m.Name == "Customer Support" || 
                                   m.ParentId == supportMenu.Id)
                        .ToListAsync();

                    foreach (var menu in managerMenus)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = supportManagerRole.Id,
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
                
                // Also give User role basic customer support access
                var userRole = await roleManager.FindByNameAsync("User");
                if (userRole != null && supportMenu != null)
                {
                    var userMenus = await context.Menus
                        .Where(m => m.Name == "Customer Support" || 
                                   m.Name == "My Tickets" || 
                                   m.Name == "Create New Ticket" ||
                                   m.Name == "Knowledge Base" ||
                                   m.Name == "FAQs")
                        .ToListAsync();

                    foreach (var menu in userMenus)
                    {
                        var roleMenu = new RoleMenu
                        {
                            RoleId = userRole.Id,
                            MenuId = menu.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = false,
                            CanDelete = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.RoleMenus.AddAsync(roleMenu);
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
    
    private static async Task SeedCustomerSupportMenusAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        // Check if Customer Support menu already exists
        var supportParentMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Customer Support");
        
        if (supportParentMenu == null)
        {
            // Create the parent menu
            supportParentMenu = new Menu
            {
                Name = "Customer Support",
                DisplayName = "Customer Support",
                Icon = "fas fa-headset",
                Order = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await context.Menus.AddAsync(supportParentMenu);
            await context.SaveChangesAsync();
        }
        
        // Check if child menus exist
        var supportChildMenusExist = await context.Menus.AnyAsync(m => m.ParentId == supportParentMenu.Id);
        
        if (!supportChildMenusExist)
        {
            var supportChildMenus = new List<Menu>
            {
                // Support Dashboard
                new Menu
                {
                    Name = "Support Dashboard",
                    DisplayName = "Support Dashboard",
                    Area = "CustomerSupport",
                    Controller = "Dashboard",
                    Action = "Index",
                    Icon = "fas fa-tachometer-alt",
                    ParentId = supportParentMenu.Id,
                    Order = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Customer Ticket Management
                new Menu
                {
                    Name = "My Tickets",
                    DisplayName = "My Tickets",
                    Area = "CustomerSupport",
                    Controller = "Ticket",
                    Action = "Index",
                    Icon = "fas fa-ticket-alt",
                    ParentId = supportParentMenu.Id,
                    Order = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Menu
                {
                    Name = "Create New Ticket",
                    DisplayName = "Create New Ticket",
                    Area = "CustomerSupport",
                    Controller = "Ticket",
                    Action = "Create",
                    Icon = "fas fa-plus-circle",
                    ParentId = supportParentMenu.Id,
                    Order = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Support Staff Functions
                new Menu
                {
                    Name = "Assigned Tickets",
                    DisplayName = "Assigned Tickets",
                    Area = "CustomerSupport",
                    Controller = "SupportTicket",
                    Action = "MyTickets",
                    Icon = "fas fa-user-check",
                    ParentId = supportParentMenu.Id,
                    Order = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Menu
                {
                    Name = "Support Queue",
                    DisplayName = "Support Queue",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Index",
                    Icon = "fas fa-clipboard-list",
                    ParentId = supportParentMenu.Id,
                    Order = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Manager Functions
                new Menu
                {
                    Name = "Ticket Management",
                    DisplayName = "Ticket Management",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Index",
                    Icon = "fas fa-tasks",
                    ParentId = supportParentMenu.Id,
                    Order = 7,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Menu
                {
                    Name = "Unassigned Tickets",
                    DisplayName = "Unassigned Tickets",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Unassigned",
                    Icon = "fas fa-exclamation-triangle",
                    ParentId = supportParentMenu.Id,
                    Order = 8,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Reports & Analytics
                new Menu
                {
                    Name = "Reports & Analytics",
                    DisplayName = "Reports & Analytics",
                    Area = "CustomerSupport",
                    Controller = "ManageTicket",
                    Action = "Statistics",
                    Icon = "fas fa-chart-bar",
                    ParentId = supportParentMenu.Id,
                    Order = 11,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            await context.Menus.AddRangeAsync(supportChildMenus);
            await context.SaveChangesAsync();
        }
        
        // Ensure role assignments for Customer Support menus
        var allSupportMenus = await context.Menus
            .Where(m => m.Name == "Customer Support" || m.ParentId == supportParentMenu.Id)
            .ToListAsync();
        
        // Assign to SuperAdmin
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        if (superAdminRole != null)
        {
            foreach (var menu in allSupportMenus)
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
                    await context.RoleMenus.AddAsync(roleMenu);
                }
            }
        }
        
        // Assign to Customer role
        var customerRole = await roleManager.FindByNameAsync("Customer");
        if (customerRole != null)
        {
            var customerMenuNames = new[] { "Customer Support", "My Tickets", "Create New Ticket" };
            var customerMenus = allSupportMenus.Where(m => customerMenuNames.Contains(m.Name)).ToList();
            
            foreach (var menu in customerMenus)
            {
                var existingRoleMenu = await context.RoleMenus
                    .FirstOrDefaultAsync(rm => rm.RoleId == customerRole.Id && rm.MenuId == menu.Id);
                
                if (existingRoleMenu == null)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = customerRole.Id,
                        MenuId = menu.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = false,
                        CanDelete = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.RoleMenus.AddAsync(roleMenu);
                }
            }
        }
        
        // Assign to Support role
        var supportRole = await roleManager.FindByNameAsync("Support");
        if (supportRole != null)
        {
            var supportMenuNames = new[] { "Customer Support", "Support Dashboard", "Assigned Tickets", "Support Queue" };
            var supportMenus = allSupportMenus.Where(m => supportMenuNames.Contains(m.Name)).ToList();
            
            foreach (var menu in supportMenus)
            {
                var existingRoleMenu = await context.RoleMenus
                    .FirstOrDefaultAsync(rm => rm.RoleId == supportRole.Id && rm.MenuId == menu.Id);
                
                if (existingRoleMenu == null)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = supportRole.Id,
                        MenuId = menu.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.RoleMenus.AddAsync(roleMenu);
                }
            }
        }
        
        // Assign to SupportManager role
        var supportManagerRole = await roleManager.FindByNameAsync("SupportManager");
        if (supportManagerRole != null)
        {
            foreach (var menu in allSupportMenus)
            {
                var existingRoleMenu = await context.RoleMenus
                    .FirstOrDefaultAsync(rm => rm.RoleId == supportManagerRole.Id && rm.MenuId == menu.Id);
                
                if (existingRoleMenu == null)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = supportManagerRole.Id,
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
        }
        
        // Also give User role basic customer support access
        var userRole = await roleManager.FindByNameAsync("User");
        if (userRole != null)
        {
            var userMenuNames = new[] { "Customer Support", "My Tickets", "Create New Ticket" };
            var userMenus = allSupportMenus.Where(m => userMenuNames.Contains(m.Name)).ToList();
            
            foreach (var menu in userMenus)
            {
                var existingRoleMenu = await context.RoleMenus
                    .FirstOrDefaultAsync(rm => rm.RoleId == userRole.Id && rm.MenuId == menu.Id);
                
                if (existingRoleMenu == null)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = userRole.Id,
                        MenuId = menu.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = false,
                        CanDelete = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.RoleMenus.AddAsync(roleMenu);
                }
            }
        }
        
        await context.SaveChangesAsync();
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
    
    private static async Task EnsureSuperAdminHasAllMenusAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        // Get SuperAdmin role
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        
        if (superAdminRole == null)
        {
            // Create SuperAdmin role if it doesn't exist
            superAdminRole = new ApplicationRole
            {
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await roleManager.CreateAsync(superAdminRole);
        }
        
        // Get ALL menus from the database
        var allMenus = await context.Menus.ToListAsync();
        
        if (!allMenus.Any())
        {
            return; // No menus to assign
        }
        
        // Ensure SuperAdmin has access to each menu
        foreach (var menu in allMenus)
        {
            // Check if the role-menu assignment already exists
            var existingRoleMenu = await context.RoleMenus
                .FirstOrDefaultAsync(rm => rm.RoleId == superAdminRole.Id && rm.MenuId == menu.Id);
            
            if (existingRoleMenu == null)
            {
                // Create new role-menu assignment with full permissions
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
            else
            {
                // Update existing assignment to ensure full permissions
                existingRoleMenu.CanView = true;
                existingRoleMenu.CanCreate = true;
                existingRoleMenu.CanEdit = true;
                existingRoleMenu.CanDelete = true;
                existingRoleMenu.UpdatedAt = DateTime.UtcNow;
                
                context.RoleMenus.Update(existingRoleMenu);
            }
        }
        
        await context.SaveChangesAsync();
        
        // Log the result
        var assignedMenuCount = await context.RoleMenus
            .Where(rm => rm.RoleId == superAdminRole.Id)
            .CountAsync();
        
        Console.WriteLine($"SuperAdmin role now has access to {assignedMenuCount} menus out of {allMenus.Count} total menus.");
    }
}