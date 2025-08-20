using Microsoft.AspNetCore.Razor.TagHelpers;
using Nexora.Application.Services;
using Nexora.Core.Constants;

namespace Nexora.Web.TagHelpers;

/// <summary>
/// Tag helper for role-based authorization in views
/// Usage: <div asp-roles="Admin,SuperAdmin">Content for admins</div>
/// </summary>
[HtmlTargetElement(Attributes = "asp-roles")]
public class RoleAuthorizationTagHelper : TagHelper
{
    private readonly IRoleAuthorizationService _roleAuthService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleAuthorizationTagHelper(
        IRoleAuthorizationService roleAuthService,
        IHttpContextAccessor httpContextAccessor)
    {
        _roleAuthService = roleAuthService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("asp-roles")]
    public string Roles { get; set; } = string.Empty;

    [HtmlAttributeName("asp-role-mode")]
    public RoleCheckMode Mode { get; set; } = RoleCheckMode.Any;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            output.SuppressOutput();
            return;
        }

        var requiredRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToArray();

        bool hasAccess = false;

        // Handle special role keywords
        if (requiredRoles.Contains("Admin"))
        {
            hasAccess = await _roleAuthService.IsAdminAsync(user);
        }
        else if (requiredRoles.Contains("Support"))
        {
            hasAccess = await _roleAuthService.IsSupportStaffAsync(user);
        }
        else
        {
            // Check specific roles
            if (Mode == RoleCheckMode.Any)
            {
                hasAccess = await _roleAuthService.IsInAnyRoleAsync(user, requiredRoles);
            }
            else // All
            {
                hasAccess = true;
                foreach (var role in requiredRoles)
                {
                    if (!await _roleAuthService.IsInRoleAsync(user, role))
                    {
                        hasAccess = false;
                        break;
                    }
                }
            }
        }

        if (!hasAccess)
        {
            output.SuppressOutput();
        }
    }
}

public enum RoleCheckMode
{
    Any,
    All
}