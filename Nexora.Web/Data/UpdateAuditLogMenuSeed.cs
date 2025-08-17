using Microsoft.EntityFrameworkCore;
using Nexora.Infrastructure.Data;

namespace Nexora.Web.Data;

public static class UpdateAuditLogMenuSeed
{
    public static async Task UpdateAuditLogMenu(ApplicationDbContext context)
    {
        // Find the existing Audit Logs menu item
        var auditLogMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "AuditLogs" && m.Controller == "Log");
        
        if (auditLogMenu != null)
        {
            // Update the controller to point to the new AuditLogController
            auditLogMenu.Controller = "AuditLog";
            auditLogMenu.DisplayName = "Audit Logs";
            await context.SaveChangesAsync();
        }
        else
        {
            // Check if AuditLog controller menu already exists
            var existingMenu = await context.Menus
                .FirstOrDefaultAsync(m => m.Controller == "AuditLog" && m.Action == "Index");
            
            if (existingMenu == null)
            {
                // Create new Audit Log menu item if it doesn't exist
                var newMenu = new Core.Entities.Menu
                {
                    Name = "AuditLogs",
                    DisplayName = "Audit Logs",
                    Controller = "AuditLog",
                    Action = "Index",
                    Icon = "fas fa-history",
                    Order = 6,
                    IsActive = true
                };
                
                context.Menus.Add(newMenu);
                await context.SaveChangesAsync();
                
                // Add permission for SuperAdmin role
                var superAdminRole = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
                
                if (superAdminRole != null)
                {
                    var roleMenu = new Core.Entities.RoleMenu
                    {
                        RoleId = superAdminRole.Id,
                        MenuId = newMenu.Id
                    };
                    
                    context.RoleMenus.Add(roleMenu);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}