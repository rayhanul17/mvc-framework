using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Application.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Nexora.Web.Extensions;

/// <summary>
/// Extension methods for checking permissions based on URLs
/// </summary>
public static class UrlPermissionExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;
    
    /// <summary>
    /// Initialize the services (called from Program.cs)
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
    }
    
    /// <summary>
    /// Check if user has permission to access a URL
    /// Usage: @if(await User.CanAccessUrlAsync("/User/Edit/123")) { ... }
    /// </summary>
    public static async Task<bool> CanAccessUrlAsync(this ClaimsPrincipal user, string url)
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
            return false;
            
        var permissionService = context.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
        
        // Parse the URL to extract area, controller, and action
        var (area, controller, action) = ParseUrl(url);
        
        if (string.IsNullOrEmpty(controller))
            return false;
            
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Check if user has permission for an action URL
    /// Usage: @if(await Html.CanAccessActionAsync("Edit", "User", new { id = 123 })) { ... }
    /// </summary>
    public static async Task<bool> CanAccessActionAsync(this IHtmlHelper html, string action, string controller, object? values = null)
    {
        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var user = html.ViewContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
        
        // Extract area from route values if present
        var area = "";
        if (values != null)
        {
            var routeValues = new RouteValueDictionary(values);
            if (routeValues.ContainsKey("area"))
            {
                area = routeValues["area"]?.ToString() ?? "";
            }
        }
        
        // If area is not in values, check current area
        if (string.IsNullOrEmpty(area))
        {
            area = html.ViewContext.RouteData.Values["area"]?.ToString() ?? "";
        }
        
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Conditionally render content based on URL permission
    /// Usage: @Html.IfCanAccess("/User/Edit/123", @<text><a href="/User/Edit/123">Edit</a></text>)
    /// </summary>
    public static async Task<IHtmlContent> IfCanAccessAsync(this IHtmlHelper html, string url, Func<object?, IHtmlContent> content)
    {
        var user = html.ViewContext.HttpContext.User;
        
        if (await user.CanAccessUrlAsync(url))
        {
            return content(null);
        }
        
        return HtmlString.Empty;
    }
    
    /// <summary>
    /// Conditionally render action link based on permission
    /// Usage: @await Html.ActionLinkIfCanAccessAsync("Edit", "Edit", "User", new { id = 123 }, new { @class = "btn btn-primary" })
    /// </summary>
    public static async Task<IHtmlContent> ActionLinkIfCanAccessAsync(
        this IHtmlHelper html,
        string linkText,
        string action,
        string controller,
        object? routeValues = null,
        object? htmlAttributes = null)
    {
        if (await html.CanAccessActionAsync(action, controller, routeValues))
        {
            return html.ActionLink(linkText, action, controller, routeValues, htmlAttributes);
        }
        
        return HtmlString.Empty;
    }
    
    /// <summary>
    /// Generate action link with automatic permission checking
    /// Usage: @await Html.SecureActionLinkAsync("Edit", "Edit", "User", new { id = 123 }, new { @class = "btn" })
    /// Returns empty if no permission
    /// </summary>
    public static async Task<IHtmlContent> SecureActionLinkAsync(
        this IHtmlHelper html,
        string linkText,
        string action,
        string? controller = null,
        object? routeValues = null,
        object? htmlAttributes = null)
    {
        controller = controller ?? html.ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
        
        if (await html.CanAccessActionAsync(action, controller, routeValues))
        {
            return html.ActionLink(linkText, action, controller, routeValues, htmlAttributes);
        }
        
        return HtmlString.Empty;
    }
    
    /// <summary>
    /// Check if current user can access the specified route
    /// Usage: @if(await Url.CanAccessRouteAsync("Edit", "User", new { area = "Admin" })) { ... }
    /// </summary>
    public static async Task<bool> CanAccessRouteAsync(this IUrlHelper url, string action, string controller, object? values = null)
    {
        var permissionService = url.ActionContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
            return false;
            
        var user = url.ActionContext.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;
        
        var area = "";
        if (values != null)
        {
            var routeValues = new RouteValueDictionary(values);
            if (routeValues.ContainsKey("area"))
            {
                area = routeValues["area"]?.ToString() ?? "";
            }
        }
        
        return await permissionService.HasPermissionAsync(user, area, controller, action);
    }
    
    /// <summary>
    /// Parse URL to extract area, controller, and action
    /// </summary>
    private static (string area, string controller, string action) ParseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return ("", "", "");
        
        // Remove query string if present
        var questionMarkIndex = url.IndexOf('?');
        if (questionMarkIndex >= 0)
        {
            url = url.Substring(0, questionMarkIndex);
        }
        
        // Remove leading slash
        url = url.TrimStart('/');
        
        // Split the URL into segments
        var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
            return ("", "Home", "Index");
        
        string area = "";
        string controller = "";
        string action = "Index";
        
        // Check if first segment is an area (common areas)
        var knownAreas = new[] { "Admin", "CustomerSupport", "Identity" };
        var firstSegmentIndex = 0;
        
        if (segments.Length > 0 && knownAreas.Contains(segments[0], StringComparer.OrdinalIgnoreCase))
        {
            area = segments[0];
            firstSegmentIndex = 1;
        }
        
        // Get controller
        if (segments.Length > firstSegmentIndex)
        {
            controller = segments[firstSegmentIndex];
        }
        else if (area != "")
        {
            controller = "Home"; // Default controller for area
        }
        else
        {
            controller = segments[0];
        }
        
        // Get action
        if (segments.Length > firstSegmentIndex + 1)
        {
            action = segments[firstSegmentIndex + 1];
            
            // If the next segment looks like an ID (number or GUID), keep action as the previous segment
            if (segments.Length > firstSegmentIndex + 2)
            {
                var possibleId = segments[firstSegmentIndex + 2];
                if (!Regex.IsMatch(possibleId, @"^\d+$") && 
                    !Regex.IsMatch(possibleId, @"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase))
                {
                    // Not an ID, might be action
                    action = segments[firstSegmentIndex + 1];
                }
            }
        }
        
        // Handle special cases
        if (controller.Equals("Account", StringComparison.OrdinalIgnoreCase))
        {
            area = "Identity";
        }
        
        return (area, controller, action);
    }
}

/// <summary>
/// HTML Helper extension for secure buttons
/// </summary>
public static class SecureButtonExtensions
{
    /// <summary>
    /// Create a button that's only visible if user has permission
    /// Usage: @await Html.SecureButtonAsync("Delete", "Delete", "User", new { id = 123 }, "Delete", "btn btn-danger", "return confirm('Are you sure?')")
    /// </summary>
    public static async Task<IHtmlContent> SecureButtonAsync(
        this IHtmlHelper html,
        string action,
        string controller,
        object? routeValues,
        string buttonText,
        string? cssClass = null,
        string? onclick = null,
        string? icon = null)
    {
        if (!await html.CanAccessActionAsync(action, controller, routeValues))
        {
            return HtmlString.Empty;
        }
        
        var url = html.ViewContext.HttpContext.Request.PathBase + 
                  "/" + controller + "/" + action;
        
        if (routeValues != null)
        {
            var routeDict = new RouteValueDictionary(routeValues);
            if (routeDict.Count > 0)
            {
                var queryString = string.Join("&", routeDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                url += "?" + queryString;
            }
        }
        
        var iconHtml = string.IsNullOrEmpty(icon) ? "" : $"<i class=\"{icon}\"></i> ";
        var onclickAttr = string.IsNullOrEmpty(onclick) ? "" : $" onclick=\"{onclick}\"";
        var classAttr = string.IsNullOrEmpty(cssClass) ? "" : $" class=\"{cssClass}\"";
        
        var buttonHtml = $"<button type=\"button\"{classAttr}{onclickAttr} data-url=\"{url}\">{iconHtml}{buttonText}</button>";
        
        return new HtmlString(buttonHtml);
    }
    
    /// <summary>
    /// Create a form with submit button that's only visible if user has permission
    /// </summary>
    public static async Task<IHtmlContent> SecureFormButtonAsync(
        this IHtmlHelper html,
        string action,
        string controller,
        object? routeValues,
        string buttonText,
        string? cssClass = null,
        string? confirmMessage = null,
        string? icon = null)
    {
        if (!await html.CanAccessActionAsync(action, controller, routeValues))
        {
            return HtmlString.Empty;
        }
        
        var formAction = html.ViewContext.HttpContext.Request.PathBase + 
                        "/" + controller + "/" + action;
        
        var iconHtml = string.IsNullOrEmpty(icon) ? "" : $"<i class=\"{icon}\"></i> ";
        var classAttr = string.IsNullOrEmpty(cssClass) ? "btn" : cssClass;
        var confirmAttr = string.IsNullOrEmpty(confirmMessage) ? "" : 
                         $" onclick=\"return confirm('{confirmMessage}')\"";
        
        var formHtml = $@"<form method=""post"" action=""{formAction}"" class=""d-inline"">
            {html.AntiForgeryToken()}
            <button type=""submit"" class=""{classAttr}""{confirmAttr}>
                {iconHtml}{buttonText}
            </button>
        </form>";
        
        return new HtmlString(formHtml);
    }
}