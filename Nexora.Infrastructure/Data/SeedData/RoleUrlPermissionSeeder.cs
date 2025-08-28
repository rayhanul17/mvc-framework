using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class RoleUrlPermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        // Get roles
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        var adminRole = await roleManager.FindByNameAsync("Administrator");
        var managerRole = await roleManager.FindByNameAsync("Manager");
        var userRole = await roleManager.FindByNameAsync("User");
        
        var permissions = new List<RoleUrlPermission>();
        
        // Administrator permissions
        if (adminRole != null)
        {
            permissions.AddRange(new[]
            {
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/admin/*", Description = "All admin pages", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/user/*", Description = "User management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/role/*", Description = "Role management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/rolepermission/*", Description = "Role permission management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/sitesetting/*", Description = "Site settings", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/menu/*", Description = "Menu management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/log/*", Description = "Log viewing", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = adminRole.Id, Url = "/permission/*", Description = "Permission management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            });
        }
        
        // Manager permissions
        if (managerRole != null)
        {
            permissions.AddRange(new[]
            {
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/admin/dashboard", Description = "Admin dashboard", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/blog/*", Description = "Blog management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/blogcategory/*", Description = "Blog category management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/blogpost/*", Description = "Blog post management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/customersupport/*", Description = "Customer support", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/ticket/*", Description = "Ticket management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = managerRole.Id, Url = "/report/*", Description = "Reports", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            });
        }
        
        // User permissions
        if (userRole != null)
        {
            permissions.AddRange(new[]
            {
                new RoleUrlPermission { RoleId = userRole.Id, Url = "/profile/*", Description = "Profile management", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = userRole.Id, Url = "/blog/view", Description = "View blogs", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = userRole.Id, Url = "/blog/details/*", Description = "View blog details", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = userRole.Id, Url = "/ticket/create", Description = "Create tickets", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new RoleUrlPermission { RoleId = userRole.Id, Url = "/ticket/my", Description = "View own tickets", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            });
        }
        
        // Add permissions if they don't exist
        foreach (var permission in permissions)
        {
            var exists = await context.RoleUrlPermissions
                .AnyAsync(rp => rp.RoleId == permission.RoleId && rp.Url == permission.Url);
                
            if (!exists)
            {
                context.RoleUrlPermissions.Add(permission);
            }
        }
        
        await context.SaveChangesAsync();
    }
}