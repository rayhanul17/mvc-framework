using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Infrastructure.Data;

namespace DynamicRoleMenuSystem.Web.Middleware;

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

                    // Skip permission check for Home and Account controllers
                    if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) || 
                        controller.Equals("Account", StringComparison.OrdinalIgnoreCase))
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
                            var hasPermission = await CheckUserPermissionAsync(dbContext, userId, area, controller, action);
                            
                            if (!hasPermission)
                            {
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

    private async Task<bool> CheckUserPermissionAsync(ApplicationDbContext dbContext, string userId, string? area, string controller, string action)
    {
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

        var hasPermission = await query
            .AnyAsync(rm => rm.Menu.Controller == controller && 
                          (string.IsNullOrEmpty(rm.Menu.Action) || rm.Menu.Action == action));

        if (!hasPermission)
        {
            hasPermission = await query
                .AnyAsync(rm => rm.Menu.Controller == controller && string.IsNullOrEmpty(rm.Menu.Action));
        }

        if (!hasPermission)
        {
            hasPermission = await query
                .AnyAsync(rm => string.IsNullOrEmpty(rm.Menu.Controller) && string.IsNullOrEmpty(rm.Menu.Action));
        }

        return hasPermission;
    }
}