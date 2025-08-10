using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface IRoleService
{
    Task<Result<IEnumerable<ApplicationRole>>> GetAllRolesAsync();
    Task<Result<ApplicationRole>> GetRoleByIdAsync(string roleId);
    Task<Result<ApplicationRole>> CreateRoleAsync(string name, string? description = null);
    Task<Result<ApplicationRole>> UpdateRoleAsync(string roleId, string name, string? description = null);
    Task<Result> DeleteRoleAsync(string roleId);
    Task<Result> AssignRoleToUserAsync(string userId, string roleId);
    Task<Result> RemoveRoleFromUserAsync(string userId, string roleId);
    Task<Result<IEnumerable<ApplicationRole>>> GetUserRolesAsync(string userId);
}