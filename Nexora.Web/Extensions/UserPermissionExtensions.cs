using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.Extensions;

/// <summary>
/// Extension methods for User ClaimsPrincipal
/// </summary>
public static class UserPermissionExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;
    
    /// <summary>
    /// Initialize the HTTP context accessor (called from Program.cs)
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
    }
    
    /// <summary>
    /// Check if user has permission
    /// Usage: @if(await User.HasPermissionAsync("", "User", "Edit")) { ... }
    /// </summary>
    public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user, string area, string controller, string action)
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
            return false;
            
        var permissionService = context.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Check if user has any permission in area
    /// Usage: @if(await User.HasAnyPermissionInAreaAsync("CustomerSupport")) { ... }
    /// </summary>
    public static async Task<bool> HasAnyPermissionInAreaAsync(this ClaimsPrincipal user, string area)
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
            return false;
            
        var permissionService = context.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.UserHasAnyPermissionInAreaAsync(user, area);
    }
    
    /// <summary>
    /// Check if user is SuperAdmin
    /// Usage: @if(await User.IsSuperAdminAsync()) { ... }
    /// </summary>
    public static async Task<bool> IsSuperAdminAsync(this ClaimsPrincipal user)
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
            return false;
            
        var permissionService = context.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.IsSuperAdminAsync(user);
    }
}