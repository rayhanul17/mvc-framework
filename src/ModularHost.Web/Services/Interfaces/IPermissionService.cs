using MRCMS.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MRCMS.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> IsUrlAllowedAsync(ClaimsPrincipal user, string url, string httpMethod = null);
        Task<bool> IsUrlAllowedAsync(Guid userId, string url, string httpMethod = null);
        Task<IEnumerable<Menu>> GetMenuForUserAsync(Guid userId);
        Task<IEnumerable<string>> GetAllowedUrlsForUserAsync(Guid userId);
        Task InvalidateCacheForUserAsync(Guid userId);
        Task InvalidateCacheForRoleAsync(Guid roleId);
        Task InvalidateAllCacheAsync();
    }
}