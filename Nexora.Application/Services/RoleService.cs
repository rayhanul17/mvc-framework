using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<IEnumerable<ApplicationRole>>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
            
            return Result<IEnumerable<ApplicationRole>>.Success(roles);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ApplicationRole>>.Failure($"Error retrieving roles: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationRole>> GetRoleByIdAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<ApplicationRole>.Failure("Role not found");

            return Result<ApplicationRole>.Success(role);
        }
        catch (Exception ex)
        {
            return Result<ApplicationRole>.Failure($"Error retrieving role: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationRole>> CreateRoleAsync(string name, string? description = null)
    {
        try
        {
            var role = new ApplicationRole
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<ApplicationRole>.Failure("Failed to create role", errors);
            }

            return Result<ApplicationRole>.Success(role);
        }
        catch (Exception ex)
        {
            return Result<ApplicationRole>.Failure($"Error creating role: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationRole>> UpdateRoleAsync(string roleId, string name, string? description = null)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<ApplicationRole>.Failure("Role not found");

            role.Name = name;
            role.Description = description;
            role.UpdatedAt = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<ApplicationRole>.Failure("Failed to update role", errors);
            }

            return Result<ApplicationRole>.Success(role);
        }
        catch (Exception ex)
        {
            return Result<ApplicationRole>.Failure($"Error updating role: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRoleAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result.Failure("Role not found");

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result.Failure("Failed to delete role", errors);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting role: {ex.Message}");
        }
    }

    public async Task<Result> AssignRoleToUserAsync(string userId, string roleId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure("User not found");

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result.Failure("Role not found");

            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result.Failure("Failed to assign role to user", errors);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning role to user: {ex.Message}");
        }
    }

    public async Task<Result> RemoveRoleFromUserAsync(string userId, string roleId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure("User not found");

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result.Failure("Role not found");

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result.Failure("Failed to remove role from user", errors);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing role from user: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ApplicationRole>>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<IEnumerable<ApplicationRole>>.Failure("User not found");

            var roleNames = await _userManager.GetRolesAsync(user);
            var roles = new List<ApplicationRole>();

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null && role.IsActive)
                    roles.Add(role);
            }

            return Result<IEnumerable<ApplicationRole>>.Success(roles);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ApplicationRole>>.Failure($"Error retrieving user roles: {ex.Message}");
        }
    }
}