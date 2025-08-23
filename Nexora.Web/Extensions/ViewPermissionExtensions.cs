using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.Extensions;

/// <summary>
/// Extension methods for checking permissions in Razor views
/// </summary>
public static class ViewPermissionExtensions
{
    /// <summary>
    /// Initialize the permission service (called from Program.cs) - No longer needed but kept for compatibility
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        // No longer storing static reference - getting from HttpContext instead
    }
    
    /// <summary>
    /// Check if current user has permission for a specific action
    /// Usage in view: @if(await Html.HasPermissionAsync("", "User", "Edit")) { ... }
    /// </summary>
    public static async Task<bool> HasPermissionAsync(this IHtmlHelper html, string area, string controller, string action)
    {
        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var user = html.ViewContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Check if current user has permission for current controller action
    /// Usage in view: @if(await Html.HasPermissionForActionAsync("Edit")) { ... }
    /// </summary>
    public static async Task<bool> HasPermissionForActionAsync(this IHtmlHelper html, string action)
    {
        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var routeData = html.ViewContext.RouteData;
        var area = routeData.Values["area"]?.ToString() ?? "";
        var controller = routeData.Values["controller"]?.ToString() ?? "";
        
        var user = html.ViewContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Check if current user has any permission in an area
    /// Usage in view: @if(await Html.HasAnyPermissionInAreaAsync("CustomerSupport")) { ... }
    /// </summary>
    public static async Task<bool> HasAnyPermissionInAreaAsync(this IHtmlHelper html, string area)
    {
        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var user = html.ViewContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.UserHasAnyPermissionInAreaAsync(user, area);
    }
    
    /// <summary>
    /// Check if current user is SuperAdmin
    /// Usage in view: @if(await Html.IsSuperAdminAsync()) { ... }
    /// </summary>
    public static async Task<bool> IsSuperAdminAsync(this IHtmlHelper html)
    {
        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var user = html.ViewContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
            
        return await permissionService.IsSuperAdminAsync(user);
    }
}