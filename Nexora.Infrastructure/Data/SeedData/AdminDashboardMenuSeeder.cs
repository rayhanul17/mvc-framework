using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class AdminDashboardMenuSeeder
{
    public static async Task SeedAdminDashboardMenuAsync(ApplicationDbContext context)
    {
        // Check if Admin Dashboard menu already exists
        var existingMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Admin Dashboard");
            
        if (existingMenu != null)
        {
            // Update the existing menu to ensure correct settings
            existingMenu.Controller = "Dashboard";
            existingMenu.Action = "Index";
            existingMenu.Area = "Admin";
            existingMenu.Url = "/Admin/Dashboard";
            existingMenu.ActiveMenuUrl = "/Admin/Dashboard";
            existingMenu.Icon = "fas fa-tachometer-alt";
            existingMenu.Order = 1;
            existingMenu.IsActive = true;
            existingMenu.AllowAnonymous = false;
            existingMenu.RequireAuthentication = true;
            
            context.Menus.Update(existingMenu);
        }
        else
        {
            // Create new Admin Dashboard menu
            var adminDashboardMenu = new Menu
            {
                Name = "Admin Dashboard",
                DisplayName = "Admin Dashboard",
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
            
            await context.Menus.AddAsync(adminDashboardMenu);
        }
        
        // Fix Settings menu to prevent false activation
        var settingsMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Settings");
            
        if (settingsMenu != null)
        {
            // Make Settings menu URL more specific
            settingsMenu.ActiveMenuUrl = "/SiteSetting/Index";
            settingsMenu.Url = "/SiteSetting/Index";
            settingsMenu.Controller = "SiteSetting";
            settingsMenu.Action = "Index";
            settingsMenu.Area = null; // Settings is not in an area
            
            context.Menus.Update(settingsMenu);
        }
        
        // Fix regular Dashboard menu if exists
        var regularDashboardMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == "Dashboard" && m.Controller == "Home");
            
        if (regularDashboardMenu != null)
        {
            regularDashboardMenu.ActiveMenuUrl = "/Home/Dashboard";
            regularDashboardMenu.Url = "/Home/Dashboard";
            
            context.Menus.Update(regularDashboardMenu);
        }
        
        await context.SaveChangesAsync();
    }
}