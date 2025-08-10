using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Models.ViewModels;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserController(
        IUserService userService,
        IRoleService roleService,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userService = userService;
        _roleService = roleService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _userService.GetAllUsersAsync();
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error loading users");
            return View(new List<UserViewModel>());
        }

        var userViewModels = result.Data!.Select(user => new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Description = user.Description,
            IsActive = user.IsActive,
            IsSuperAdmin = user.IsSuperAdmin,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            AssignedRoles = user.UserRoles.Select(ur => ur.Role.Name!).ToList()
        }).ToList();

        return View(userViewModels);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var result = await _userService.GetUserByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return NotFound();
        }

        var user = result.Data;
        var roles = await _userManager.GetRolesAsync(user);

        var viewModel = new UserDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Description = user.Description,
            IsActive = user.IsActive,
            IsSuperAdmin = user.IsSuperAdmin,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            AssignedRoles = roles.ToList()
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return RedirectToAction(nameof(CreateEdit));
    }

    public IActionResult Edit(string id)
    {
        return RedirectToAction(nameof(CreateEdit), new { id });
    }

    public async Task<IActionResult> CreateEdit(string? id = null)
    {
        var isEditMode = !string.IsNullOrEmpty(id);
        
        if (isEditMode)
        {
            var result = await _userService.GetUserByIdAsync(id!);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var user = result.Data;
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();

            var viewModel = new UserCreateEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Description = user.Description,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                SelectedRoleIds = allRoles.Where(r => userRoles.Contains(r.Name!)).Select(r => r.Id).ToList(),
                IsEditMode = true
            };

            await PopulateRolesViewBag();
            return View("CreateEdit", viewModel);
        }
        else
        {
            await PopulateRolesViewBag();
            return View("CreateEdit", new UserCreateEditViewModel { IsEditMode = false });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(UserCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRolesViewBag();
            return View("CreateEdit", model);
        }

        if (string.IsNullOrEmpty(model.Id)) // Create mode
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required for new users");
                await PopulateRolesViewBag();
                return View("CreateEdit", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                AvatarUrl = model.AvatarUrl,
                Description = model.Description,
                IsActive = model.IsActive,
                IsSuperAdmin = model.IsSuperAdmin,
                EmailConfirmed = true
            };

            var result = await _userService.CreateUserAsync(user, model.Password!);
            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage ?? "Error creating user");
                await PopulateRolesViewBag();
                return View("CreateEdit", model);
            }

            if (model.SelectedRoleIds?.Any() == true)
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name!);
                    }
                }
            }

            SetSuccessMessage("User created successfully");
            return RedirectToAction(nameof(Index));
        }
        else // Edit mode
        {
            var result = await _userService.GetUserByIdAsync(model.Id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var user = result.Data;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.AvatarUrl = model.AvatarUrl;
            user.Description = model.Description;
            user.IsActive = model.IsActive;
            user.IsSuperAdmin = model.IsSuperAdmin;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;

            var updateResult = await _userService.UpdateUserAsync(user);
            if (!updateResult.IsSuccess)
            {
                SetErrorMessage(updateResult.ErrorMessage ?? "Error updating user");
                await PopulateRolesViewBag();
                return View("CreateEdit", model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.ToList();
            var rolesToAdd = new List<string>();

            if (model.SelectedRoleIds != null)
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        if (!currentRoles.Contains(role.Name!))
                        {
                            rolesToAdd.Add(role.Name!);
                        }
                        else
                        {
                            rolesToRemove.Remove(role.Name!);
                        }
                    }
                }
            }

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            SetSuccessMessage("User updated successfully");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var result = await _userService.DeleteUserAsync(id);
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error deleting user");
        }
        else
        {
            SetSuccessMessage("User deleted successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var userResult = await _userService.GetUserByIdAsync(id);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return NotFound();
        }

        var user = userResult.Data;
        var result = user.IsActive
            ? await _userService.DeactivateUserAsync(id)
            : await _userService.ActivateUserAsync(id);

        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error updating user status");
        }
        else
        {
            var status = user.IsActive ? "deactivated" : "activated";
            SetSuccessMessage($"User {status} successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesViewBag()
    {
        var rolesResult = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = rolesResult.Data ?? Enumerable.Empty<ApplicationRole>();
    }
}