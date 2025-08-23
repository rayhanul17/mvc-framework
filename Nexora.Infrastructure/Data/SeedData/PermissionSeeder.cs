using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var permissions = new List<Permission>
        {
            // Blog Management Permissions
            new Permission { Name = "BlogPost.View", Area = "", Controller = "BlogPost", Action = "Index", Description = "View blog posts", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogPost.Create", Area = "", Controller = "BlogPost", Action = "Create", Description = "Create blog posts", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogPost.Edit", Area = "", Controller = "BlogPost", Action = "Edit", Description = "Edit blog posts", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogPost.Delete", Area = "", Controller = "BlogPost", Action = "Delete", Description = "Delete blog posts", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogPost.Details", Area = "", Controller = "BlogPost", Action = "Details", Description = "View blog post details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "BlogCategory.View", Area = "", Controller = "BlogCategory", Action = "Index", Description = "View blog categories", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogCategory.Create", Area = "", Controller = "BlogCategory", Action = "Create", Description = "Create blog categories", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogCategory.Edit", Area = "", Controller = "BlogCategory", Action = "Edit", Description = "Edit blog categories", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogCategory.Delete", Area = "", Controller = "BlogCategory", Action = "Delete", Description = "Delete blog categories", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "BlogTag.View", Area = "", Controller = "BlogTag", Action = "Index", Description = "View blog tags", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogTag.Create", Area = "", Controller = "BlogTag", Action = "Create", Description = "Create blog tags", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogTag.Edit", Area = "", Controller = "BlogTag", Action = "Edit", Description = "Edit blog tags", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogTag.Delete", Area = "", Controller = "BlogTag", Action = "Delete", Description = "Delete blog tags", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "BlogImport.View", Area = "", Controller = "BlogImport", Action = "Index", Description = "View blog import/export", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogImport.Import", Area = "", Controller = "BlogImport", Action = "Import", Description = "Import blog data", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "BlogImport.Export", Area = "", Controller = "BlogImport", Action = "Export", Description = "Export blog data", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // User Management Permissions
            new Permission { Name = "User.View", Area = "", Controller = "User", Action = "Index", Description = "View users", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "User.Create", Area = "", Controller = "User", Action = "Create", Description = "Create users", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "User.Edit", Area = "", Controller = "User", Action = "Edit", Description = "Edit users", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "User.Delete", Area = "", Controller = "User", Action = "Delete", Description = "Delete users", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "User.Details", Area = "", Controller = "User", Action = "Details", Description = "View user details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Role.View", Area = "", Controller = "Role", Action = "Index", Description = "View roles", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Role.Create", Area = "", Controller = "Role", Action = "Create", Description = "Create roles", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Role.Edit", Area = "", Controller = "Role", Action = "Edit", Description = "Edit roles", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Role.Delete", Area = "", Controller = "Role", Action = "Delete", Description = "Delete roles", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Menu.View", Area = "", Controller = "Menu", Action = "Index", Description = "View menus", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Menu.Create", Area = "", Controller = "Menu", Action = "Create", Description = "Create menus", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Menu.Edit", Area = "", Controller = "Menu", Action = "Edit", Description = "Edit menus", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Menu.Delete", Area = "", Controller = "Menu", Action = "Delete", Description = "Delete menus", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Settings Permissions
            new Permission { Name = "SiteSetting.View", Area = "", Controller = "SiteSetting", Action = "Index", Description = "View site settings", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "SiteSetting.Edit", Area = "", Controller = "SiteSetting", Action = "Edit", Description = "Edit site settings", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "FileDocument.View", Area = "", Controller = "FileDocument", Action = "Index", Description = "View file documents", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "FileDocument.Upload", Area = "", Controller = "FileDocument", Action = "Upload", Description = "Upload file documents", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "FileDocument.Delete", Area = "", Controller = "FileDocument", Action = "Delete", Description = "Delete file documents", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Audit Log Permissions
            new Permission { Name = "Log.View", Area = "", Controller = "Log", Action = "Index", Description = "View audit logs", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Log.Details", Area = "", Controller = "Log", Action = "Details", Description = "View audit log details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Log.Export", Area = "", Controller = "Log", Action = "Export", Description = "Export audit logs", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Customer Support Permissions
            new Permission { Name = "Support.Dashboard", Area = "CustomerSupport", Controller = "Dashboard", Action = "Index", Description = "View support dashboard", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Home", Area = "CustomerSupport", Controller = "Home", Action = "Index", Description = "View support home", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Support.Ticket.View", Area = "CustomerSupport", Controller = "Ticket", Action = "Index", Description = "View tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Ticket.Create", Area = "CustomerSupport", Controller = "Ticket", Action = "Create", Description = "Create tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Ticket.Edit", Area = "CustomerSupport", Controller = "Ticket", Action = "Edit", Description = "Edit tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Ticket.Details", Area = "CustomerSupport", Controller = "Ticket", Action = "Details", Description = "View ticket details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Ticket.Dashboard", Area = "CustomerSupport", Controller = "Ticket", Action = "Dashboard", Description = "View ticket dashboard", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Ticket.Track", Area = "CustomerSupport", Controller = "Ticket", Action = "Track", Description = "Track tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Support.ManageTicket.View", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Index", Description = "Manage tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.ManageTicket.Assign", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Assign", Description = "Assign tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.ManageTicket.Close", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Close", Description = "Close tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.ManageTicket.Statistics", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Statistics", Description = "View ticket statistics", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.ManageTicket.Unassigned", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Unassigned", Description = "View unassigned tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.ManageTicket.Details", Area = "CustomerSupport", Controller = "ManageTicket", Action = "Details", Description = "View ticket details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Support.SupportTicket.MyTickets", Area = "CustomerSupport", Controller = "SupportTicket", Action = "MyTickets", Description = "View my support tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.SupportTicket.Details", Area = "CustomerSupport", Controller = "SupportTicket", Action = "Details", Description = "View support ticket details", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.SupportTicket.Resolve", Area = "CustomerSupport", Controller = "SupportTicket", Action = "Resolve", Description = "Resolve support tickets", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            new Permission { Name = "Support.Configuration.View", Area = "CustomerSupport", Controller = "Configuration", Action = "Index", Description = "View support configuration", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.Configuration.Edit", Area = "CustomerSupport", Controller = "Configuration", Action = "Edit", Description = "Edit support configuration", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Dashboard Permission
            new Permission { Name = "Dashboard.View", Area = "", Controller = "Home", Action = "Dashboard", Description = "View dashboard", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Admin Area Permissions
            new Permission { Name = "Admin.Dashboard", Area = "Admin", Controller = "Dashboard", Action = "Index", Description = "View admin dashboard", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Permission Management Permissions
            new Permission { Name = "Permission.View", Area = "", Controller = "Permission", Action = "Index", Description = "View permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.Create", Area = "", Controller = "Permission", Action = "Create", Description = "Create permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.Edit", Area = "", Controller = "Permission", Action = "Edit", Description = "Edit permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.Delete", Area = "", Controller = "Permission", Action = "Delete", Description = "Delete permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.RolePermissions", Area = "", Controller = "Permission", Action = "RolePermissions", Description = "Manage role permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.UserPermissions", Area = "", Controller = "Permission", Action = "UserPermissions", Description = "View user permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.UpdateRolePermissions", Area = "", Controller = "Permission", Action = "UpdateRolePermissions", Description = "Update role permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.BulkAssign", Area = "", Controller = "Permission", Action = "BulkAssign", Description = "Bulk assign permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.ClearCache", Area = "", Controller = "Permission", Action = "ClearCache", Description = "Clear permission cache", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.CacheStatus", Area = "", Controller = "Permission", Action = "CacheStatus", Description = "View cache status", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.TestDashboard", Area = "", Controller = "Permission", Action = "TestDashboard", Description = "Access permission testing dashboard", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.TestPermission", Area = "", Controller = "Permission", Action = "TestPermission", Description = "Test individual permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Permission.TestBulkPermissions", Area = "", Controller = "Permission", Action = "TestBulkPermissions", Description = "Test bulk permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Comment Management Permissions
            new Permission { Name = "Comment.View", Area = "", Controller = "Comment", Action = "Index", Description = "View comments", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Comment.Create", Area = "", Controller = "Comment", Action = "Create", Description = "Create comments", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Comment.Edit", Area = "", Controller = "Comment", Action = "Edit", Description = "Edit comments", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Comment.Delete", Area = "", Controller = "Comment", Action = "Delete", Description = "Delete comments", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            
            // Wildcard permissions for broader access
            new Permission { Name = "Blog.All", Area = "", Controller = "Blog", Action = "*", Description = "All blog permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" },
            new Permission { Name = "Support.All", Area = "CustomerSupport", Controller = "*", Action = "*", Description = "All support permissions", IsActive = true, CreatedAt = now, CreatedBy = "System" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
    
    public static async Task AssignPermissionsToRolesAsync(ApplicationDbContext context)
    {
        // Check if role permissions already exist
        if (await context.RolePermissions.AnyAsync())
            return;
            
        // Get roles
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        var supportRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Support");
        var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
        
        if (adminRole == null)
            return;
            
        var rolePermissions = new List<RolePermission>();
        
        // Assign Blog permissions to Administrator
        var blogPermissions = await context.Permissions
            .Where(p => p.Controller.StartsWith("Blog") || p.Name.StartsWith("Blog"))
            .ToListAsync();
            
        foreach (var permission in blogPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign User Management permissions to Administrator
        var userPermissions = await context.Permissions
            .Where(p => p.Controller == "User" || p.Controller == "Role")
            .ToListAsync();
            
        foreach (var permission in userPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Menu Management permissions to Administrator
        var menuPermissions = await context.Permissions
            .Where(p => p.Controller == "Menu")
            .ToListAsync();
            
        foreach (var permission in menuPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Site Settings permissions to Administrator
        var settingsPermissions = await context.Permissions
            .Where(p => p.Controller == "SiteSetting" || p.Controller == "FileDocument")
            .ToListAsync();
            
        foreach (var permission in settingsPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Log View permission to Administrator
        var logPermissions = await context.Permissions
            .Where(p => p.Controller == "Log" && (p.Action == "Index" || p.Action == "Details" || p.Action == "Export"))
            .ToListAsync();
            
        foreach (var permission in logPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Dashboard permission to Administrator
        var dashboardPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Name == "Dashboard.View");
            
        if (dashboardPermission != null)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = dashboardPermission.Id,
                IsActive = true
            });
        }
        
        // Assign Admin Dashboard permission to Administrator
        var adminDashboardPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Name == "Admin.Dashboard");
            
        if (adminDashboardPermission != null)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = adminDashboardPermission.Id,
                IsActive = true
            });
        }
        
        // Assign Permission Management permissions to Administrator
        var permissionManagementPermissions = await context.Permissions
            .Where(p => p.Controller == "Permission")
            .ToListAsync();
            
        foreach (var permission in permissionManagementPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Comment Management permissions to Administrator
        var commentPermissions = await context.Permissions
            .Where(p => p.Controller == "Comment")
            .ToListAsync();
            
        foreach (var permission in commentPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Create Support role if it doesn't exist
        if (supportRole == null)
        {
            supportRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Support",
                NormalizedName = "SUPPORT",
                Description = "Customer support role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Roles.AddAsync(supportRole);
            await context.SaveChangesAsync();
        }
        
        // Assign Support permissions to Support role
        var supportPermissions = await context.Permissions
            .Where(p => p.Area == "CustomerSupport" || p.Name.StartsWith("Support"))
            .ToListAsync();
            
        foreach (var permission in supportPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = supportRole.Id,
                PermissionId = permission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        // Assign Dashboard permission to Support role
        if (dashboardPermission != null)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = supportRole.Id,
                PermissionId = dashboardPermission.Id,
                IsActive = true
            });
        }
        
        // Create Manager role if it doesn't exist
        if (managerRole == null)
        {
            managerRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Manager",
                NormalizedName = "MANAGER",
                Description = "Manager role with extended permissions",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Roles.AddAsync(managerRole);
            await context.SaveChangesAsync();
        }
        
        // Assign Manager permissions (Similar to Admin but without some critical permissions)
        if (managerRole != null)
        {
            // Blog management
            foreach (var permission in blogPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
            
            // User management (View and Edit only, no Delete)
            var managerUserPermissions = await context.Permissions
                .Where(p => p.Controller == "User" && (p.Action == "Index" || p.Action == "Details" || p.Action == "Edit" || p.Action == "Create"))
                .ToListAsync();
                
            foreach (var permission in managerUserPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
            
            // File Document management
            var filePermissions = await context.Permissions
                .Where(p => p.Controller == "FileDocument")
                .ToListAsync();
                
            foreach (var permission in filePermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
            
            // Dashboard access
            if (dashboardPermission != null)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = dashboardPermission.Id,
                    IsActive = true
                });
            }
        }
        
        // Assign basic permissions to User role
        if (userRole != null && dashboardPermission != null)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = userRole.Id,
                PermissionId = dashboardPermission.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
        
        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }
}