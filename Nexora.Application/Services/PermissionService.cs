using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string area, string controller, string action)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await HasPermissionAsync(userId, area, controller, action);
    }

    public async Task<bool> HasPermissionAsync(string userId, string area, string controller, string action)
    {
        // Check if user is SuperAdmin
        var user = await _unitOfWork.Repository<Core.Entities.ApplicationUser>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
            return false;
        
        // SuperAdmin bypasses all permission checks
        if (user.IsSuperAdmin)
            return true;
        
        // Check if user is active
        if (!user.IsActive)
            return false;

        // Get user's active roles
        var now = DateTime.UtcNow;
        var userRoleIds = await _unitOfWork.Repository<Core.Entities.UserRole>()
            .GetQueryable()
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && 
                        ur.IsActive && 
                        (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        // Check if any of user's roles have permission for this action
        var hasPermission = await _unitOfWork.Repository<Core.Entities.RolePermission>()
            .GetQueryable()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive)
            .AnyAsync(rp => 
                (string.IsNullOrEmpty(area) || rp.Permission.Area == area) &&
                rp.Permission.Controller == controller &&
                rp.Permission.Action == action);

        // If no exact match, check for wildcard action permission
        if (!hasPermission)
        {
            hasPermission = await _unitOfWork.Repository<Core.Entities.RolePermission>()
                .GetQueryable()
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    (string.IsNullOrEmpty(area) || rp.Permission.Area == area) &&
                    rp.Permission.Controller == controller &&
                    rp.Permission.Action == "*");
        }

        // If still no match, check for area-wide permission
        if (!hasPermission && !string.IsNullOrEmpty(area))
        {
            hasPermission = await _unitOfWork.Repository<Core.Entities.RolePermission>()
                .GetQueryable()
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    rp.Permission.Area == area &&
                    rp.Permission.Controller == "*" &&
                    rp.Permission.Action == "*");
        }

        return hasPermission;
    }

    public async Task<bool> UserHasAnyPermissionInAreaAsync(ClaimsPrincipal user, string area)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await UserHasAnyPermissionInAreaAsync(userId, area);
    }

    public async Task<bool> UserHasAnyPermissionInAreaAsync(string userId, string area)
    {
        // Check if user is SuperAdmin
        if (await IsSuperAdminAsync(userId))
            return true;

        // Get user's active roles
        var now = DateTime.UtcNow;
        var userRoleIds = await _unitOfWork.Repository<Core.Entities.UserRole>()
            .GetQueryable()
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && 
                        ur.IsActive && 
                        (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return false;

        // Check if any of user's roles have any permission in this area
        return await _unitOfWork.Repository<Core.Entities.RolePermission>()
            .GetQueryable()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive &&
                        rp.Permission.Area == area)
            .AnyAsync();
    }

    public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await IsSuperAdminAsync(userId);
    }

    public async Task<bool> IsSuperAdminAsync(string userId)
    {
        var user = await _unitOfWork.Repository<Core.Entities.ApplicationUser>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        return user?.IsSuperAdmin ?? false;
    }
}