using Microsoft.AspNetCore.Identity;
using Nexora.Core.Entities;
using System.Security.Claims;

namespace Nexora.Application.Services;

public interface IRoleAuthorizationService
{
    Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role);
    Task<bool> IsInAnyRoleAsync(ClaimsPrincipal user, params string[] roles);
    Task<bool> IsSuperAdminAsync(ClaimsPrincipal user);
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string area, string controller, string action);
    Task<List<string>> GetUserRolesAsync(ClaimsPrincipal user);
}

public class RoleAuthorizationService : IRoleAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleAuthorizationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null)
            return false;

        return await _userManager.IsInRoleAsync(appUser, role);
    }

    public async Task<bool> IsInAnyRoleAsync(ClaimsPrincipal user, params string[] roles)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null)
            return false;

        foreach (var role in roles)
        {
            if (await _userManager.IsInRoleAsync(appUser, role))
                return true;
        }

        return false;
    }

    public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null)
            return false;

        // Check the IsSuperAdmin field directly
        return appUser.IsSuperAdmin;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string area, string controller, string action)
    {
        // This method would check the Permission table
        // For now, we check if user is SuperAdmin or has specific roles
        if (await IsSuperAdminAsync(user))
            return true;

        // Additional permission logic would go here
        return false;
    }

    public async Task<List<string>> GetUserRolesAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return new List<string>();

        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null)
            return new List<string>();

        var roles = await _userManager.GetRolesAsync(appUser);
        return roles.ToList();
    }
}