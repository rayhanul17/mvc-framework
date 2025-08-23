using System.Security.Claims;

namespace Nexora.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string area, string controller, string action);
    Task<bool> HasPermissionAsync(string userId, string area, string controller, string action);
    Task<bool> UserHasAnyPermissionInAreaAsync(ClaimsPrincipal user, string area);
    Task<bool> UserHasAnyPermissionInAreaAsync(string userId, string area);
    Task<bool> IsSuperAdminAsync(ClaimsPrincipal user);
    Task<bool> IsSuperAdminAsync(string userId);
}