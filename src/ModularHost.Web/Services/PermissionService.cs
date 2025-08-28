using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ModularHost.Web.Core.Models.Entities;
using ModularHost.Web.Core.Infrastructure;
using ModularHost.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModularHost.Web.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public PermissionService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> IsUrlAllowedAsync(ClaimsPrincipal user, string url, string httpMethod = null)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return false;

            return await IsUrlAllowedAsync(userId, url, httpMethod);
        }

        public async Task<bool> IsUrlAllowedAsync(Guid userId, string url, string httpMethod = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            if (user.IsSuperAdmin)
                return true;

            var cacheKey = $"user_permissions_{userId}";
            var allowedUrls = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
                return await GetAllowedUrlsForUserInternalAsync(userId);
            });

            url = url?.ToLower().TrimEnd('/');
            httpMethod = httpMethod?.ToUpper();

            return allowedUrls.Any(allowed =>
            {
                var parts = allowed.Split('|');
                var allowedUrl = parts[0].ToLower().TrimEnd('/');
                var allowedMethod = parts.Length > 1 ? parts[1] : null;

                if (!UrlMatches(url, allowedUrl))
                    return false;

                return string.IsNullOrEmpty(allowedMethod) || 
                       string.IsNullOrEmpty(httpMethod) || 
                       allowedMethod == httpMethod;
            });
        }

        private bool UrlMatches(string requestUrl, string patternUrl)
        {
            if (requestUrl == patternUrl)
                return true;

            if (patternUrl.Contains("*"))
            {
                var pattern = "^" + patternUrl.Replace("*", ".*") + "$";
                return System.Text.RegularExpressions.Regex.IsMatch(requestUrl, pattern);
            }

            return requestUrl.StartsWith(patternUrl + "/");
        }

        public async Task<IEnumerable<Menu>> GetMenuForUserAsync(Guid userId)
        {
            var cacheKey = $"user_menu_{userId}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
                
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                    return Enumerable.Empty<Menu>();

                if (user.IsSuperAdmin)
                {
                    return await _context.Menus
                        .Where(m => m.IsVisible)
                        .OrderBy(m => m.Order)
                        .ToListAsync();
                }

                var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
                var allowedUrls = await _context.RolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Url)
                    .Distinct()
                    .ToListAsync();

                return await _context.Menus
                    .Where(m => m.IsVisible && (string.IsNullOrEmpty(m.Url) || allowedUrls.Contains(m.Url)))
                    .OrderBy(m => m.Order)
                    .ToListAsync();
            });
        }

        public async Task<IEnumerable<string>> GetAllowedUrlsForUserAsync(Guid userId)
        {
            var cacheKey = $"user_permissions_{userId}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
                return await GetAllowedUrlsForUserInternalAsync(userId);
            });
        }

        private async Task<List<string>> GetAllowedUrlsForUserInternalAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
                return new List<string>();

            if (user.IsSuperAdmin)
                return new List<string> { "*" };

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            
            var permissions = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => new { rp.Url, rp.HttpMethod })
                .Distinct()
                .ToListAsync();

            return permissions
                .Select(p => string.IsNullOrEmpty(p.HttpMethod) ? p.Url : $"{p.Url}|{p.HttpMethod}")
                .ToList();
        }

        public async Task InvalidateCacheForUserAsync(Guid userId)
        {
            _cache.Remove($"user_permissions_{userId}");
            _cache.Remove($"user_menu_{userId}");
            await Task.CompletedTask;
        }

        public async Task InvalidateCacheForRoleAsync(Guid roleId)
        {
            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await InvalidateCacheForUserAsync(userId);
            }
        }

        public async Task InvalidateAllCacheAsync()
        {
            var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
            foreach (var userId in userIds)
            {
                await InvalidateCacheForUserAsync(userId);
            }
        }
    }
}