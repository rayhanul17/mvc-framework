using Nexora.Application.Services;
using System.Security.Claims;

namespace Nexora.Web.Extensions;

public static class UserExtensions
{
    private static IRoleAuthorizationService? _roleAuthService;
    
    public static void Configure(IRoleAuthorizationService roleAuthService)
    {
        _roleAuthService = roleAuthService;
    }
    
    public static async Task<bool> IsInRoleAsync(this ClaimsPrincipal user, string role)
    {
        if (_roleAuthService == null)
            throw new InvalidOperationException("RoleAuthorizationService not configured");
            
        return await _roleAuthService.IsInRoleAsync(user, role);
    }
    
    public static async Task<bool> IsSuperAdminAsync(this ClaimsPrincipal user)
    {
        if (_roleAuthService == null)
            throw new InvalidOperationException("RoleAuthorizationService not configured");
            
        return await _roleAuthService.IsSuperAdminAsync(user);
    }
    
    public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user, string area, string controller, string action)
    {
        if (_roleAuthService == null)
            throw new InvalidOperationException("RoleAuthorizationService not configured");
            
        return await _roleAuthService.HasPermissionAsync(user, area, controller, action);
    }
}