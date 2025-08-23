using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Infrastructure.Data;

namespace Nexora.Web.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionMiddleware> _logger;

    public PermissionMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<PermissionMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Check for AllowAnonymous attribute - skip all checks
        var allowAnonymous = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        // Check for Authorize attribute
        var authorizeAttribute = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
        var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        
        // If no Authorize attribute and no controller action, continue
        if (authorizeAttribute == null && controllerActionDescriptor == null)
        {
            await _next(context);
            return;
        }

        // If user is not authenticated and Authorize attribute exists, redirect to login
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            if (authorizeAttribute != null)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
            await _next(context);
            return;
        }

        // User is authenticated, now check permissions
        if (controllerActionDescriptor != null)
        {
            var area = controllerActionDescriptor.RouteValues.ContainsKey("area") 
                ? controllerActionDescriptor.RouteValues["area"] 
                : string.Empty;
            var controller = controllerActionDescriptor.ControllerName;
            var action = controllerActionDescriptor.ActionName;

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }

                // Check if user has permission
                var hasPermission = await CheckUserPermissionAsync(dbContext, userId, area, controller, action);
                
                if (!hasPermission)
                {
                    _logger.LogWarning($"Access denied for user {userId} to {area}/{controller}/{action}");
                    
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Access Denied");
                    }
                    else
                    {
                        context.Response.Redirect("/Error/AccessDenied");
                    }
                    return;
                }
            }
        }

        await _next(context);
    }

    private async Task<bool> CheckUserPermissionAsync(ApplicationDbContext dbContext, string userId, string? area, string controller, string action)
    {
        // First check if user exists and has IsSuperAdmin flag
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
            return false;
        
        // SuperAdmin bypasses all permission checks
        if (user.IsSuperAdmin)
            return true;
        
        // Check if user is active
        if (!user.IsActive)
            return false;
        
        // Get user's active roles (considering expiration)
        var now = DateTime.UtcNow;
        var userRoleIds = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId && 
                        ur.IsActive && 
                        (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        // Check if any of user's roles have permission for this action
        var hasPermission = await dbContext.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive)
            .AnyAsync(rp => 
                (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                rp.Permission.Controller == controller &&
                rp.Permission.Action == action);

        // If no exact match, check for wildcard action permission (all actions in controller)
        if (!hasPermission)
        {
            hasPermission = await dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                    rp.Permission.Controller == controller &&
                    rp.Permission.Action == "*"); // Wildcard for all actions
        }

        // If still no match, check for area-wide permission
        if (!hasPermission && !string.IsNullOrEmpty(area))
        {
            hasPermission = await dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    rp.Permission.Area == area &&
                    rp.Permission.Controller == "*" &&
                    rp.Permission.Action == "*"); // Wildcard for all controllers and actions in area
        }

        return hasPermission;
    }
}