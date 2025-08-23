using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Nexora.Infrastructure.Data;
using System.Security.Claims;

namespace Nexora.Web.Attributes;

/// <summary>
/// Dynamic permission-based authorization attribute that checks user permissions
/// instead of hardcoded roles
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAuthorizeAttribute : TypeFilterAttribute
{
    public PermissionAuthorizeAttribute(string? area = null, string? controller = null, string? action = null) 
        : base(typeof(PermissionAuthorizeFilter))
    {
        Arguments = new object[] { area ?? "", controller ?? "", action ?? "" };
    }
}

public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;
    private readonly string _area;
    private readonly string _controller;
    private readonly string _action;

    public PermissionAuthorizeFilter(
        ApplicationDbContext context,
        string area,
        string controller,
        string action)
    {
        _context = context;
        _area = area;
        _controller = controller;
        _action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // Allow anonymous access if AllowAnonymous attribute is present
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            return;
        }

        // Check if user is authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if user has permission
        var hasPermission = await CheckUserPermissionAsync(userId);
        
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }

    private async Task<bool> CheckUserPermissionAsync(string userId)
    {
        // Check if user is SuperAdmin
        var user = await _context.Users
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

        // If no specific permission is required (controller/action not specified), 
        // just check if user is authenticated
        if (string.IsNullOrEmpty(_controller) && string.IsNullOrEmpty(_action))
            return true;

        // Get user's active roles
        var now = DateTime.UtcNow;
        var userRoleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId && 
                        ur.IsActive && 
                        (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        // Check if any of user's roles have permission for this action
        var hasPermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive)
            .AnyAsync(rp => 
                (string.IsNullOrEmpty(_area) || rp.Permission.Area == _area) &&
                (string.IsNullOrEmpty(_controller) || rp.Permission.Controller == _controller) &&
                (string.IsNullOrEmpty(_action) || rp.Permission.Action == _action));

        // If no exact match, check for wildcard action permission
        if (!hasPermission && !string.IsNullOrEmpty(_controller))
        {
            hasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    (string.IsNullOrEmpty(_area) || rp.Permission.Area == _area) &&
                    rp.Permission.Controller == _controller &&
                    rp.Permission.Action == "*");
        }

        // If still no match, check for area-wide permission
        if (!hasPermission && !string.IsNullOrEmpty(_area))
        {
            hasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    rp.Permission.Area == _area &&
                    rp.Permission.Controller == "*" &&
                    rp.Permission.Action == "*");
        }

        return hasPermission;
    }
}

/// <summary>
/// Convenience attribute for requiring authentication without specific permissions
/// </summary>
public class RequireAuthenticationAttribute : PermissionAuthorizeAttribute
{
    public RequireAuthenticationAttribute() : base(null, null, null)
    {
    }
}

/// <summary>
/// Attribute to check permissions dynamically based on current action
/// </summary>
public class DynamicPermissionAuthorizeAttribute : TypeFilterAttribute
{
    public DynamicPermissionAuthorizeAttribute() : base(typeof(DynamicPermissionAuthorizeFilter))
    {
    }
}

public class DynamicPermissionAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;

    public DynamicPermissionAuthorizeFilter(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // Allow anonymous access if AllowAnonymous attribute is present
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            return;
        }

        // Check if user is authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get current controller and action from route data
        var routeData = context.RouteData;
        var area = routeData.Values["area"]?.ToString() ?? "";
        var controller = routeData.Values["controller"]?.ToString() ?? "";
        var action = routeData.Values["action"]?.ToString() ?? "";

        // Check if user has permission
        var hasPermission = await CheckUserPermissionAsync(userId, area, controller, action);
        
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }

    private async Task<bool> CheckUserPermissionAsync(string userId, string area, string controller, string action)
    {
        // Check if user is SuperAdmin
        var user = await _context.Users
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

        // Get user's active roles
        var now = DateTime.UtcNow;
        var userRoleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId && 
                        ur.IsActive && 
                        (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        // Check if any of user's roles have permission for this action
        var hasPermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive)
            .AnyAsync(rp => 
                (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                rp.Permission.Controller == controller &&
                rp.Permission.Action == action);

        // If no exact match, check for wildcard action permission
        if (!hasPermission)
        {
            hasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                    rp.Permission.Controller == controller &&
                    rp.Permission.Action == "*");
        }

        // If still no match, check for area-wide permission
        if (!hasPermission && !string.IsNullOrEmpty(area))
        {
            hasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    rp.Permission.Area == area &&
                    rp.Permission.Controller == "*" &&
                    rp.Permission.Action == "*");
        }

        return hasPermission;
    }
}