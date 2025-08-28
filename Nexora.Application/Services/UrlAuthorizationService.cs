using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;
using System.Security.Claims;

namespace Nexora.Application.Services;

public class UrlAuthorizationService : IUrlAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UrlAuthorizationService(
        IUnitOfWork unitOfWork, 
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<bool> HasAccessToUrlAsync(string userId, string url)
    {
        // Get user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;
            
        // SuperAdmin has access to everything
        if (user.IsSuperAdmin)
            return true;
        
        // Normalize URL
        url = NormalizeUrl(url);
        
        // Get user's roles
        var userRoles = await _userManager.GetRolesAsync(user);
        
        // Check if any of user's roles has permission for this URL
        var hasPermission = await _unitOfWork.Repository<RoleUrlPermission>()
            .GetQueryable()
            .Include(rp => rp.Role)
            .AnyAsync(rp => userRoles.Contains(rp.Role.Name) && 
                           rp.IsActive && 
                           IsUrlMatch(url, rp.Url));
        
        return hasPermission;
    }
    
    public async Task<bool> HasAccessToActionAsync(string userId, string area, string controller, string action)
    {
        // Build URL from controller action
        var url = BuildUrl(area, controller, action);
        return await HasAccessToUrlAsync(userId, url);
    }
    
    public async Task<List<string>> GetUserPermissionUrlsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new List<string>();
            
        // SuperAdmin gets all URLs
        if (user.IsSuperAdmin)
            return new List<string> { "*" };
        
        // Get user's roles
        var userRoles = await _userManager.GetRolesAsync(user);
        
        // Get all permission URLs for user's roles
        var urls = await _unitOfWork.Repository<RoleUrlPermission>()
            .GetQueryable()
            .Include(rp => rp.Role)
            .Where(rp => userRoles.Contains(rp.Role.Name) && rp.IsActive)
            .Select(rp => rp.Url)
            .Distinct()
            .ToListAsync();
        
        return urls;
    }
    
    public async Task<bool> AddRolePermissionAsync(string roleId, string url, string? description = null)
    {
        try
        {
            // Check if permission already exists
            var exists = await _unitOfWork.Repository<RoleUrlPermission>()
                .GetQueryable()
                .AnyAsync(rp => rp.RoleId == roleId && rp.Url == url);
                
            if (exists)
                return false;
            
            var permission = new RoleUrlPermission
            {
                RoleId = roleId,
                Url = NormalizeUrl(url),
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            
            await _unitOfWork.Repository<RoleUrlPermission>().AddAsync(permission);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> RemoveRolePermissionAsync(string roleId, string url)
    {
        try
        {
            var permission = await _unitOfWork.Repository<RoleUrlPermission>()
                .GetQueryable()
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Url == url);
                
            if (permission == null)
                return false;
            
            _unitOfWork.Repository<RoleUrlPermission>().Remove(permission);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "/";
            
        url = url.ToLower().Trim();
        
        // Ensure URL starts with /
        if (!url.StartsWith("/"))
            url = "/" + url;
            
        // Remove trailing slash except for root
        if (url.Length > 1 && url.EndsWith("/"))
            url = url.TrimEnd('/');
            
        return url;
    }
    
    private bool IsUrlMatch(string requestUrl, string permissionUrl)
    {
        // Exact match
        if (requestUrl.Equals(permissionUrl, StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Wildcard match (e.g., /admin/* matches /admin/dashboard)
        if (permissionUrl.EndsWith("*"))
        {
            var baseUrl = permissionUrl.TrimEnd('*');
            return requestUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase);
        }
        
        return false;
    }
    
    private string BuildUrl(string? area, string controller, string action)
    {
        if (!string.IsNullOrEmpty(area))
            return $"/{area}/{controller}/{action}".ToLower();
        else
            return $"/{controller}/{action}".ToLower();
    }
}