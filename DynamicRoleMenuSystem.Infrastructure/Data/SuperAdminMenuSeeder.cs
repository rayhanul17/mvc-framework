using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Infrastructure.Data;

public static class SuperAdminMenuSeeder
{
    public static async Task SeedAllMenusForSuperAdminAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        // Get or create SuperAdmin role
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        if (superAdminRole == null)
        {
            superAdminRole = new ApplicationRole
            {
                Name = "SuperAdmin",
                Description = "Super Administrator with full system access",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await roleManager.CreateAsync(superAdminRole);
        }

        // Seed comprehensive menu structure
        await SeedComprehensiveMenusAsync(context);
        
        // Ensure SuperAdmin has access to ALL menus
        await AssignAllMenusToSuperAdminAsync(context, superAdminRole);
    }

    private static async Task SeedComprehensiveMenusAsync(ApplicationDbContext context)
    {
        var menuData = new List<(Menu menu, string? parentName)>
        {
            // Dashboard
            (new Menu { Name = "Dashboard", DisplayName = "Dashboard", Controller = "Home", Action = "Index", Icon = "fas fa-tachometer-alt", Order = 1, IsActive = true }, null),
            
            // Blog Management
            (new Menu { Name = "Blog Management", DisplayName = "Blog Management", Icon = "fas fa-blog", Order = 2, IsActive = true }, null),
            (new Menu { Name = "Blog Categories", DisplayName = "Blog Categories", Controller = "BlogCategory", Action = "Index", Icon = "fas fa-folder", Order = 1, IsActive = true }, "Blog Management"),
            (new Menu { Name = "Blog Posts", DisplayName = "Blog Posts", Controller = "BlogPost", Action = "Index", Icon = "fas fa-newspaper", Order = 2, IsActive = true }, "Blog Management"),
            (new Menu { Name = "Blog Comments", DisplayName = "Blog Comments", Controller = "BlogComment", Action = "Index", Icon = "fas fa-comments", Order = 3, IsActive = true }, "Blog Management"),
            (new Menu { Name = "Blog Tags", DisplayName = "Blog Tags", Controller = "BlogTag", Action = "Index", Icon = "fas fa-tags", Order = 4, IsActive = true }, "Blog Management"),
            
            // Administration
            (new Menu { Name = "Administration", DisplayName = "Administration", Icon = "fas fa-cogs", Order = 3, IsActive = true }, null),
            (new Menu { Name = "User Management", DisplayName = "User Management", Controller = "User", Action = "Index", Icon = "fas fa-users", Order = 1, IsActive = true }, "Administration"),
            (new Menu { Name = "Role Management", DisplayName = "Role Management", Controller = "Role", Action = "Index", Icon = "fas fa-user-tag", Order = 2, IsActive = true }, "Administration"),
            (new Menu { Name = "Menu Management", DisplayName = "Menu Management", Controller = "Menu", Action = "Index", Icon = "fas fa-bars", Order = 3, IsActive = true }, "Administration"),
            (new Menu { Name = "Site Settings", DisplayName = "Site Settings", Controller = "SiteSetting", Action = "Index", Icon = "fas fa-sliders-h", Order = 4, IsActive = true }, "Administration"),
            (new Menu { Name = "Audit Logs", DisplayName = "Audit Logs", Controller = "AuditLog", Action = "Index", Icon = "fas fa-history", Order = 5, IsActive = true }, "Administration"),
            (new Menu { Name = "System Configuration", DisplayName = "System Configuration", Controller = "SystemConfig", Action = "Index", Icon = "fas fa-cog", Order = 6, IsActive = true }, "Administration"),
            (new Menu { Name = "Email Settings", DisplayName = "Email Settings", Controller = "EmailSettings", Action = "Index", Icon = "fas fa-envelope", Order = 7, IsActive = true }, "Administration"),
            (new Menu { Name = "Backup & Restore", DisplayName = "Backup & Restore", Controller = "Backup", Action = "Index", Icon = "fas fa-database", Order = 8, IsActive = true }, "Administration"),
            
            // Customer Support
            (new Menu { Name = "Customer Support", DisplayName = "Customer Support", Area = "CustomerSupport", Icon = "fas fa-headset", Order = 4, IsActive = true }, null),
            (new Menu { Name = "Support Dashboard", DisplayName = "Support Dashboard", Area = "CustomerSupport", Controller = "Dashboard", Action = "Index", Icon = "fas fa-chart-line", Order = 1, IsActive = true }, "Customer Support"),
            (new Menu { Name = "My Tickets", DisplayName = "My Tickets", Area = "CustomerSupport", Controller = "Ticket", Action = "Index", Icon = "fas fa-ticket-alt", Order = 2, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Create New Ticket", DisplayName = "Create New Ticket", Area = "CustomerSupport", Controller = "Ticket", Action = "Create", Icon = "fas fa-plus-circle", Order = 3, IsActive = true }, "Customer Support"),
            (new Menu { Name = "All Tickets", DisplayName = "All Tickets", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Icon = "fas fa-list", Order = 4, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Assigned Tickets", DisplayName = "Assigned Tickets", Area = "CustomerSupport", Controller = "SupportTicket", Action = "MyTickets", Icon = "fas fa-user-check", Order = 5, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Unassigned Tickets", DisplayName = "Unassigned Tickets", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Unassigned", Icon = "fas fa-inbox", Order = 6, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Support Queue", DisplayName = "Support Queue", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Icon = "fas fa-tasks", Order = 7, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Pending Review", DisplayName = "Pending Review", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Icon = "fas fa-clock", Order = 8, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Escalated Tickets", DisplayName = "Escalated Tickets", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Icon = "fas fa-arrow-up", Order = 9, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Overdue Tickets", DisplayName = "Overdue Tickets", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Icon = "fas fa-hourglass-end", Order = 10, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Reports & Analytics", DisplayName = "Reports & Analytics", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Statistics", Icon = "fas fa-chart-bar", Order = 11, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Performance Metrics", DisplayName = "Performance Metrics", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Statistics", Icon = "fas fa-chart-line", Order = 12, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Agent Statistics", DisplayName = "Agent Statistics", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Statistics", Icon = "fas fa-user-chart", Order = 13, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Notifications", DisplayName = "Notifications", Area = "CustomerSupport", Controller = "Dashboard", Action = "Notifications", Icon = "fas fa-bell", Order = 14, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Support Settings", DisplayName = "Support Settings", Area = "CustomerSupport", Controller = "Settings", Action = "Index", Icon = "fas fa-cog", Order = 15, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Ticket Categories", DisplayName = "Ticket Categories", Area = "CustomerSupport", Controller = "Settings", Action = "Categories", Icon = "fas fa-tags", Order = 16, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Email Templates", DisplayName = "Email Templates", Area = "CustomerSupport", Controller = "Settings", Action = "EmailTemplates", Icon = "fas fa-envelope", Order = 17, IsActive = true }, "Customer Support"),
            (new Menu { Name = "SLA Configuration", DisplayName = "SLA Configuration", Area = "CustomerSupport", Controller = "Settings", Action = "SLA", Icon = "fas fa-stopwatch", Order = 18, IsActive = true }, "Customer Support"),
            (new Menu { Name = "Knowledge Base", DisplayName = "Knowledge Base", Area = "CustomerSupport", Controller = "KnowledgeBase", Action = "Index", Icon = "fas fa-book", Order = 19, IsActive = true }, "Customer Support"),
            (new Menu { Name = "FAQs", DisplayName = "FAQs", Area = "CustomerSupport", Controller = "KnowledgeBase", Action = "FAQ", Icon = "fas fa-question-circle", Order = 20, IsActive = true }, "Customer Support"),
            
            // Reports & Analytics
            (new Menu { Name = "Reports", DisplayName = "Reports & Analytics", Icon = "fas fa-chart-pie", Order = 5, IsActive = true }, null),
            (new Menu { Name = "User Reports", DisplayName = "User Reports", Controller = "Reports", Action = "Users", Icon = "fas fa-user-chart", Order = 1, IsActive = true }, "Reports"),
            (new Menu { Name = "Activity Reports", DisplayName = "Activity Reports", Controller = "Reports", Action = "Activity", Icon = "fas fa-chart-line", Order = 2, IsActive = true }, "Reports"),
            (new Menu { Name = "System Reports", DisplayName = "System Reports", Controller = "Reports", Action = "System", Icon = "fas fa-server", Order = 3, IsActive = true }, "Reports"),
            (new Menu { Name = "Export Data", DisplayName = "Export Data", Controller = "Reports", Action = "Export", Icon = "fas fa-download", Order = 4, IsActive = true }, "Reports"),
            
            // Security
            (new Menu { Name = "Security", DisplayName = "Security", Icon = "fas fa-shield-alt", Order = 6, IsActive = true }, null),
            (new Menu { Name = "Login History", DisplayName = "Login History", Controller = "Security", Action = "LoginHistory", Icon = "fas fa-sign-in-alt", Order = 1, IsActive = true }, "Security"),
            (new Menu { Name = "Security Settings", DisplayName = "Security Settings", Controller = "Security", Action = "Settings", Icon = "fas fa-lock", Order = 2, IsActive = true }, "Security"),
            (new Menu { Name = "Two-Factor Auth", DisplayName = "Two-Factor Authentication", Controller = "Security", Action = "TwoFactor", Icon = "fas fa-mobile-alt", Order = 3, IsActive = true }, "Security"),
            (new Menu { Name = "Access Control", DisplayName = "Access Control", Controller = "Security", Action = "AccessControl", Icon = "fas fa-key", Order = 4, IsActive = true }, "Security"),
            
            // Profile
            (new Menu { Name = "Profile", DisplayName = "Profile", Icon = "fas fa-user-circle", Order = 7, IsActive = true }, null),
            (new Menu { Name = "My Profile", DisplayName = "My Profile", Controller = "Profile", Action = "Index", Icon = "fas fa-user", Order = 1, IsActive = true }, "Profile"),
            (new Menu { Name = "Edit Profile", DisplayName = "Edit Profile", Controller = "Profile", Action = "Edit", Icon = "fas fa-user-edit", Order = 2, IsActive = true }, "Profile"),
            (new Menu { Name = "Change Password", DisplayName = "Change Password", Controller = "Profile", Action = "ChangePassword", Icon = "fas fa-key", Order = 3, IsActive = true }, "Profile"),
            (new Menu { Name = "Preferences", DisplayName = "Preferences", Controller = "Profile", Action = "Preferences", Icon = "fas fa-cog", Order = 4, IsActive = true }, "Profile"),
            
            // Help & Documentation
            (new Menu { Name = "Help", DisplayName = "Help & Documentation", Icon = "fas fa-question-circle", Order = 8, IsActive = true }, null),
            (new Menu { Name = "Documentation", DisplayName = "Documentation", Controller = "Help", Action = "Documentation", Icon = "fas fa-book", Order = 1, IsActive = true }, "Help"),
            (new Menu { Name = "API Reference", DisplayName = "API Reference", Controller = "Help", Action = "ApiReference", Icon = "fas fa-code", Order = 2, IsActive = true }, "Help"),
            (new Menu { Name = "Video Tutorials", DisplayName = "Video Tutorials", Controller = "Help", Action = "Videos", Icon = "fas fa-video", Order = 3, IsActive = true }, "Help"),
            (new Menu { Name = "Support Center", DisplayName = "Support Center", Controller = "Help", Action = "Support", Icon = "fas fa-life-ring", Order = 4, IsActive = true }, "Help")
        };

        // Process parent menus first
        foreach (var (menu, parentName) in menuData.Where(m => m.parentName == null))
        {
            await EnsureMenuExistsAsync(context, menu, null);
        }

        // Process child menus
        foreach (var (menu, parentName) in menuData.Where(m => m.parentName != null))
        {
            var parent = await context.Menus.FirstOrDefaultAsync(m => m.Name == parentName);
            if (parent != null)
            {
                await EnsureMenuExistsAsync(context, menu, parent.Id);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureMenuExistsAsync(ApplicationDbContext context, Menu menu, int? parentId)
    {
        var existingMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == menu.Name && m.ParentId == parentId);

        if (existingMenu == null)
        {
            menu.ParentId = parentId;
            menu.CreatedAt = DateTime.UtcNow;
            await context.Menus.AddAsync(menu);
        }
        else
        {
            // Update existing menu if needed
            existingMenu.DisplayName = menu.DisplayName;
            existingMenu.Controller = menu.Controller;
            existingMenu.Action = menu.Action;
            existingMenu.Area = menu.Area;
            existingMenu.Icon = menu.Icon;
            existingMenu.Order = menu.Order;
            existingMenu.IsActive = menu.IsActive;
            existingMenu.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static async Task AssignAllMenusToSuperAdminAsync(ApplicationDbContext context, ApplicationRole superAdminRole)
    {
        var allMenus = await context.Menus.ToListAsync();

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
                await context.RoleMenus.AddAsync(roleMenu);
            }
            else
            {
                // Ensure SuperAdmin has full permissions
                existingRoleMenu.CanView = true;
                existingRoleMenu.CanCreate = true;
                existingRoleMenu.CanEdit = true;
                existingRoleMenu.CanDelete = true;
                existingRoleMenu.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }
}