using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface IUserService : IBaseService<ApplicationUser>
{
    Task<Result<IEnumerable<ApplicationUser>>> GetAllUsersAsync();
    Task<Result<ApplicationUser>> GetUserByIdAsync(string id);
    Task<Result<ApplicationUser>> GetUserByEmailAsync(string email);
    Task<Result<ApplicationUser>> CreateUserAsync(ApplicationUser user, string password);
    Task<Result<ApplicationUser>> UpdateUserAsync(ApplicationUser user);
    Task<Result<bool>> DeleteUserAsync(string id);
    Task<Result<bool>> ActivateUserAsync(string id);
    Task<Result<bool>> DeactivateUserAsync(string id);
    Task<Result<IEnumerable<ApplicationUser>>> GetActiveUsersAsync();
    Task<Result<IEnumerable<ApplicationUser>>> GetUsersByRoleAsync(string roleId);
}