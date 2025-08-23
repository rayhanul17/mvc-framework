using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nexora.Core.Entities;
using System.Security.Claims;

namespace Nexora.Application.Services;

/// <summary>
/// Authorization handler that checks the IsSuperAdmin field instead of role names
/// </summary>
public class SuperAdminRequirement : IAuthorizationRequirement { }

public class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuperAdminAuthorizationHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);
        
        if (user != null && user.IsSuperAdmin)
        {
            context.Succeed(requirement);
        }
    }
}

/// <summary>
/// Extension methods for authorization
/// </summary>
public static class AuthorizationExtensions
{
    public static async Task<bool> IsSuperAdminAsync(this UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        return user?.IsSuperAdmin ?? false;
    }
    
    public static async Task<bool> IsActiveAsync(this UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        return user?.IsActive ?? false;
    }
}