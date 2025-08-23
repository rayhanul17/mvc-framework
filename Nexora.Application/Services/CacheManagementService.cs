using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Nexora.Application.Services;

public interface ICacheManagementService
{
    void RegisterCacheKey(string key);
    void ClearUserCache(string userId);
    void ClearRoleCache(string roleId);
    void ClearAllPermissionCaches();
    int GetCachedItemCount();
}

public class CacheManagementService : ICacheManagementService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public CacheManagementService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void RegisterCacheKey(string key)
    {
        _cacheKeys.TryAdd(key, true);
    }

    public void ClearUserCache(string userId)
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(k => k.Contains(userId))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
    }

    public void ClearRoleCache(string roleId)
    {
        // Clear all permission caches when role changes
        // Since role changes affect all users with that role
        ClearAllPermissionCaches();
    }

    public void ClearAllPermissionCaches()
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(k => k.StartsWith("permission_") || 
                       k.StartsWith("superadmin_") || 
                       k.StartsWith("user_permissions_") ||
                       k.StartsWith("any_area_permission_"))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
    }

    public int GetCachedItemCount()
    {
        return _cacheKeys.Count;
    }
}