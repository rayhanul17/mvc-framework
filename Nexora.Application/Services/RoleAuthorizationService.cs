using Microsoft.AspNetCore.Identity;
using Nexora.Core.Constants;
using Nexora.Core.Entities;
using System.Security.Claims;

namespace Nexora.Application.Services;

public interface IRoleAuthorizationService
{
    Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role);
    Task<bool> IsInAnyRoleAsync(ClaimsPrincipal user, params string[] roles);
    Task<bool> IsAdminAsync(ClaimsPrincipal user);
    Task<bool> IsSuperAdminAsync(ClaimsPrincipal user);
    Task<bool> IsSupportStaffAsync(ClaimsPrincipal user);
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

    public async Task<bool> IsAdminAsync(ClaimsPrincipal user)
    {
        return await IsInAnyRoleAsync(user, RoleConstants.AdminRoles);
    }

    public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal user)
    {
        return await IsInRoleAsync(user, RoleConstants.SuperAdmin);
    }

    public async Task<bool> IsSupportStaffAsync(ClaimsPrincipal user)
    {
        return await IsInAnyRoleAsync(user, RoleConstants.SupportRoles);
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