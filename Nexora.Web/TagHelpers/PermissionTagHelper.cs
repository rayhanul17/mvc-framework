using Microsoft.AspNetCore.Razor.TagHelpers;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.TagHelpers;

/// <summary>
/// Tag helper for showing/hiding elements based on user permissions
/// Usage: <div permission-area="CustomerSupport" permission-controller="Ticket" permission-action="Create">...</div>
/// Or: <a permission-controller="User" permission-action="Edit">Edit</a>
/// </summary>
[HtmlTargetElement(Attributes = "permission-controller")]
[HtmlTargetElement(Attributes = "permission-action")]
[HtmlTargetElement(Attributes = "permission-area")]
public class PermissionTagHelper : TagHelper
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("permission-area")]
    public string? Area { get; set; }

    [HtmlAttributeName("permission-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeName("permission-action")]
    public string? Action { get; set; }

    [HtmlAttributeName("permission-require-all")]
    public bool RequireAll { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
            return;
        }

        // If no specific permission is specified, just check authentication
        if (string.IsNullOrEmpty(Area) && string.IsNullOrEmpty(Controller) && string.IsNullOrEmpty(Action))
        {
            return; // User is authenticated, show the element
        }

        var hasPermission = await _permissionService.HasPermissionAsync(
            user, 
            Area ?? "", 
            Controller ?? "", 
            Action ?? ""
        );

        if (!hasPermission)
        {
            output.SuppressOutput();
        }
    }
}

/// <summary>
/// Tag helper for checking if user has any permission in an area
/// Usage: <div permission-any-in-area="CustomerSupport">...</div>
/// </summary>
[HtmlTargetElement(Attributes = "permission-any-in-area")]
public class PermissionAnyInAreaTagHelper : TagHelper
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAnyInAreaTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("permission-any-in-area")]
    public string Area { get; set; } = "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
            return;
        }

        var hasPermission = await _permissionService.UserHasAnyPermissionInAreaAsync(user, Area);

        if (!hasPermission)
        {
            output.SuppressOutput();
        }
    }
}

/// <summary>
/// Tag helper for showing elements only to SuperAdmin
/// Usage: <div permission-superadmin-only="true">...</div>
/// </summary>
[HtmlTargetElement(Attributes = "permission-superadmin-only")]
public class SuperAdminOnlyTagHelper : TagHelper
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuperAdminOnlyTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("permission-superadmin-only")]
    public bool RequireSuperAdmin { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!RequireSuperAdmin)
            return;

        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
            return;
        }

        var isSuperAdmin = await _permissionService.IsSuperAdminAsync(user);

        if (!isSuperAdmin)
        {
            output.SuppressOutput();
        }
    }
}

/// <summary>
/// Tag helper for showing elements only to authenticated users
/// Usage: <div permission-authenticated-only="true">...</div>
/// </summary>
[HtmlTargetElement(Attributes = "permission-authenticated-only")]
public class AuthenticatedOnlyTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedOnlyTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("permission-authenticated-only")]
    public bool RequireAuthentication { get; set; } = true;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!RequireAuthentication)
            return Task.CompletedTask;

        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Tag helper for showing elements only to anonymous users
/// Usage: <div permission-anonymous-only="true">...</div>
/// </summary>
[HtmlTargetElement(Attributes = "permission-anonymous-only")]
public class AnonymousOnlyTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AnonymousOnlyTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("permission-anonymous-only")]
    public bool RequireAnonymous { get; set; } = true;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!RequireAnonymous)
            return Task.CompletedTask;

        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
        }

        return Task.CompletedTask;
    }
}