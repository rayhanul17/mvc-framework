using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class UserService : BaseService<ApplicationUser>, IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        : base(unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<IEnumerable<ApplicationUser>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Result<IEnumerable<ApplicationUser>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ApplicationUser>>.Failure($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationUser>> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return Result<ApplicationUser>.Failure("User not found");

            return Result<ApplicationUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<ApplicationUser>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationUser>> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<ApplicationUser>.Failure("User not found");

            return Result<ApplicationUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<ApplicationUser>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationUser>> CreateUserAsync(ApplicationUser user, string password)
    {
        try
        {
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<ApplicationUser>.Failure($"Error creating user: {errors}");
            }

            return Result<ApplicationUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<ApplicationUser>.Failure($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationUser>> UpdateUserAsync(ApplicationUser user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<ApplicationUser>.Failure($"Error updating user: {errors}");
            }

            return Result<ApplicationUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<ApplicationUser>.Failure($"Error updating user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Error deleting user: {errors}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ActivateUserAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result<bool>.Failure("User not found");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Error activating user: {errors}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error activating user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeactivateUserAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result<bool>.Failure("User not found");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Error deactivating user: {errors}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deactivating user: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ApplicationUser>>> GetActiveUsersAsync()
    {
        try
        {
            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Result<IEnumerable<ApplicationUser>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ApplicationUser>>.Failure($"Error retrieving active users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ApplicationUser>>> GetUsersByRoleAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<IEnumerable<ApplicationUser>>.Failure("Role not found");

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            return Result<IEnumerable<ApplicationUser>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ApplicationUser>>.Failure($"Error retrieving users by role: {ex.Message}");
        }
    }
}