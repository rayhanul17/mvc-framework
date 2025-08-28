namespace Nexora.Application.Interfaces;

public interface IUrlAuthorizationService
{
    Task<bool> HasAccessToUrlAsync(string userId, string url);
    Task<bool> HasAccessToActionAsync(string userId, string area, string controller, string action);
    Task<List<string>> GetUserPermissionUrlsAsync(string userId);
    Task<bool> AddRolePermissionAsync(string roleId, string url, string? description = null);
    Task<bool> RemoveRolePermissionAsync(string roleId, string url);
}