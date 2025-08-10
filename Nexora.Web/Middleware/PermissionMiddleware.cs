using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Infrastructure.Data;

namespace Nexora.Web.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public PermissionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                {
                    var area = controllerActionDescriptor.RouteValues.ContainsKey("area") 
                        ? controllerActionDescriptor.RouteValues["area"] 
                        : null;
                    var controller = controllerActionDescriptor.ControllerName;
                    var action = controllerActionDescriptor.ActionName;

                    // Skip permission check for certain controllers
                    var skipControllers = new[] { "Home", "Account", "Profile", "Admin", "Help" };
                    if (skipControllers.Any(c => controller.Equals(c, StringComparison.OrdinalIgnoreCase)))
                    {
                        await _next(context);
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            // Quick check for SuperAdmin first to avoid unnecessary database queries
                            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                            if (user != null && user.IsSuperAdmin)
                            {
                                await _next(context);
                                return;
                            }
                            
                            var hasPermission = await CheckUserPermissionAsync(dbContext, userId, area, controller, action, user);
                            
                            if (!hasPermission)
                            {
                                // Log the access denied for debugging
                                var logger = scope.ServiceProvider.GetService<ILogger<PermissionMiddleware>>();
                                logger?.LogWarning($"Access denied for user {userId} to {area}/{controller}/{action}");
                                
                                context.Response.Redirect("/Account/AccessDenied");
                                return;
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
    }

    private async Task<bool> CheckUserPermissionAsync(ApplicationDbContext dbContext, string userId, string? area, string controller, string action, Nexora.Core.Entities.ApplicationUser? user = null)
    {
        // If user was already fetched, use it, otherwise fetch it
        if (user == null)
        {
            user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
        
        // First check if user has IsSuperAdmin flag - This overrides all permissions
        if (user != null && user.IsSuperAdmin)
            return true; // User with IsSuperAdmin flag has access to everything
        
        // Then check if user is in SuperAdmin role
        var isSuperAdmin = await dbContext.UserRoles
            .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .AnyAsync(x => x.UserId == userId && x.Name == "SuperAdmin");
        
        if (isSuperAdmin)
            return true; // SuperAdmin role has access to everything
        
        var userRoleIds = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        var query = dbContext.RoleMenus
            .Include(rm => rm.Menu)
            .Where(rm => userRoleIds.Contains(rm.RoleId) && rm.CanView && rm.Menu.IsActive);

        if (!string.IsNullOrEmpty(area))
        {
            query = query.Where(rm => rm.Menu.Area == area);
        }
        else
        {
            query = query.Where(rm => string.IsNullOrEmpty(rm.Menu.Area));
        }

        // Check for exact match first
        var hasPermission = await query
            .AnyAsync(rm => rm.Menu.Controller == controller && 
                          (string.IsNullOrEmpty(rm.Menu.Action) || rm.Menu.Action == action));

        // If no exact match, check if user has access to the controller (any action)
        if (!hasPermission)
        {
            hasPermission = await query
                .AnyAsync(rm => rm.Menu.Controller == controller && string.IsNullOrEmpty(rm.Menu.Action));
        }

        // If still no match, check if user has access to parent menu for the area
        if (!hasPermission && !string.IsNullOrEmpty(area))
        {
            hasPermission = await query
                .AnyAsync(rm => string.IsNullOrEmpty(rm.Menu.Controller) && 
                              string.IsNullOrEmpty(rm.Menu.Action) && 
                              rm.Menu.Area == area);
        }

        // If still no match, check if user has general access to the area
        if (!hasPermission)
        {
            hasPermission = await query
                .AnyAsync(rm => string.IsNullOrEmpty(rm.Menu.Controller) && string.IsNullOrEmpty(rm.Menu.Action));
        }
        
        // Special case: If user has access to ManageTicket controller, they should have access to all its actions
        if (!hasPermission && controller == "ManageTicket" && area == "CustomerSupport")
        {
            hasPermission = await query
                .AnyAsync(rm => rm.Menu.Controller == "ManageTicket" || 
                              (rm.Menu.Area == "CustomerSupport" && string.IsNullOrEmpty(rm.Menu.Controller)));
        }

        return hasPermission;
    }
}