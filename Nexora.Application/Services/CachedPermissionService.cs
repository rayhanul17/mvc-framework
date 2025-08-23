using Microsoft.Extensions.Caching.Memory;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Application.Services;

/// <summary>
/// Cached implementation of IPermissionService for improved performance
/// </summary>
public class CachedPermissionService : IPermissionService
{
    private readonly IPermissionService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ICacheManagementService _cacheManagement;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public CachedPermissionService(PermissionService innerService, IMemoryCache cache, ICacheManagementService cacheManagement)
    {
        _innerService = innerService;
        _cache = cache;
        _cacheManagement = cacheManagement;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string area, string controller, string action)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        // Check SuperAdmin status with cache
        var isSuperAdmin = await GetCachedSuperAdminStatus(userId);
        if (isSuperAdmin)
            return true;

        // Generate cache key for permission check
        var cacheKey = GeneratePermissionCacheKey(userId, area, controller, action);
        _cacheManagement.RegisterCacheKey(cacheKey);
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return await _innerService.HasPermissionAsync(user, area, controller, action);
        });
    }

    public async Task<bool> HasPermissionAsync(string userId, string area, string controller, string action)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        // Check SuperAdmin status with cache
        var isSuperAdmin = await GetCachedSuperAdminStatus(userId);
        if (isSuperAdmin)
            return true;

        var cacheKey = GeneratePermissionCacheKey(userId, area, controller, action);
        _cacheManagement.RegisterCacheKey(cacheKey);
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return await _innerService.HasPermissionAsync(userId, area, controller, action);
        });
    }

    public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await GetCachedSuperAdminStatus(userId);
    }

    public async Task<bool> IsSuperAdminAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        return await GetCachedSuperAdminStatus(userId);
    }

    // Removed methods that don't exist in IPermissionService interface
    // HasAreaPermissionAsync, GetUserPermissionsAsync, GetRolePermissionsAsync

    public async Task<bool> UserHasAnyPermissionInAreaAsync(ClaimsPrincipal user, string area)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await UserHasAnyPermissionInAreaAsync(userId, area);
    }

    public async Task<bool> UserHasAnyPermissionInAreaAsync(string userId, string area)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        // Check SuperAdmin status with cache
        var isSuperAdmin = await GetCachedSuperAdminStatus(userId);
        if (isSuperAdmin)
            return true;

        var cacheKey = $"any_area_permission_{userId}_{area}";
        _cacheManagement.RegisterCacheKey(cacheKey);
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return await _innerService.UserHasAnyPermissionInAreaAsync(userId, area);
        });
    }

    /// <summary>
    /// Clear all permission caches for a specific user
    /// </summary>
    public void ClearUserCache(string userId)
    {
        _cacheManagement.ClearUserCache(userId);
    }

    /// <summary>
    /// Clear all permission caches
    /// </summary>
    public void ClearAllCaches()
    {
        _cacheManagement.ClearAllPermissionCaches();
    }

    private async Task<bool> GetCachedSuperAdminStatus(string userId)
    {
        var cacheKey = $"superadmin_{userId}";
        _cacheManagement.RegisterCacheKey(cacheKey);
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return await _innerService.IsSuperAdminAsync(userId);
        });
    }

    private string GeneratePermissionCacheKey(string userId, string area, string controller, string action)
    {
        return $"permission_{userId}_{area ?? "noArea"}_{controller}_{action}";
    }
}