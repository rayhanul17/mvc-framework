using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class FixDashboardMenuSeeder
{
    public static async Task FixDashboardMenusAsync(ApplicationDbContext context)
    {
        // Remove old regular Dashboard menu (Home/Dashboard)
        var oldDashboardMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Dashboard" && m.Controller == "Home");
            
        if (oldDashboardMenu != null)
        {
            context.Menus.Remove(oldDashboardMenu);
        }
        
        // Remove duplicate Admin Dashboard if exists
        var duplicateAdminDashboard = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Admin Dashboard");
            
        if (duplicateAdminDashboard != null)
        {
            context.Menus.Remove(duplicateAdminDashboard);
        }
        
        // Check if Dashboard menu exists, update it or create new one
        var dashboardMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Dashboard");
            
        if (dashboardMenu != null)
        {
            // Update existing Dashboard menu to point to Admin Dashboard
            dashboardMenu.DisplayName = "Dashboard";
            dashboardMenu.Controller = "Dashboard";
            dashboardMenu.Action = "Index";
            dashboardMenu.Area = "Admin";
            dashboardMenu.Url = "/Admin/Dashboard";
            dashboardMenu.ActiveMenuUrl = "/Admin/Dashboard";
            dashboardMenu.Icon = "fas fa-tachometer-alt";
            dashboardMenu.Order = 1;
            dashboardMenu.IsActive = true;
            dashboardMenu.AllowAnonymous = false;
            dashboardMenu.RequireAuthentication = true;
            
            context.Menus.Update(dashboardMenu);
        }
        else
        {
            // Create new Dashboard menu pointing to Admin Dashboard
            var newDashboardMenu = new Menu
            {
                Name = "Dashboard",
                DisplayName = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Area = "Admin",
                Url = "/Admin/Dashboard",
                ActiveMenuUrl = "/Admin/Dashboard",
                Icon = "fas fa-tachometer-alt",
                Order = 1,
                IsActive = true,
                AllowAnonymous = false,
                RequireAuthentication = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            
            await context.Menus.AddAsync(newDashboardMenu);
        }
        
        // Fix Settings menu to prevent false activation
        var settingsMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Settings");
            
        if (settingsMenu != null)
        {
            settingsMenu.ActiveMenuUrl = "/SiteSetting";
            settingsMenu.Url = "/SiteSetting";
            settingsMenu.Controller = "SiteSetting";
            settingsMenu.Action = "Index";
            settingsMenu.Area = null;
            settingsMenu.Order = 10; // Move settings down in order
            
            context.Menus.Update(settingsMenu);
        }
        
        // Fix other menus order
        var userMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "User Management");
        if (userMenu != null)
        {
            userMenu.Order = 2;
            context.Menus.Update(userMenu);
        }
        
        var roleMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Role Management");
        if (roleMenu != null)
        {
            roleMenu.Order = 3;
            context.Menus.Update(roleMenu);
        }
        
        var blogMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Blog Management");
        if (blogMenu != null)
        {
            blogMenu.Order = 4;
            context.Menus.Update(blogMenu);
        }
        
        var ticketMenu = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Customer Support");
        if (ticketMenu != null)
        {
            ticketMenu.Order = 5;
            context.Menus.Update(ticketMenu);
        }
        
        await context.SaveChangesAsync();
    }
}