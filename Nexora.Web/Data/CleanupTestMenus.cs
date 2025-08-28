using Microsoft.EntityFrameworkCore;
using Nexora.Infrastructure.Data;

namespace Nexora.Web.Data;

public static class CleanupTestMenus
{
    public static async Task RemoveTestMenusAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Remove any test or check menus
        var testMenus = await context.Menus
            .Where(m => m.Name.Contains("Test") || 
                       m.Name.Contains("Check") || 
                       m.Name.Contains("test") || 
                       m.Name.Contains("check") ||
                       m.Name == "CheckMenus")
            .ToListAsync();
        
        if (testMenus.Any())
        {
            context.Menus.RemoveRange(testMenus);
            await context.SaveChangesAsync();
        }
    }
}