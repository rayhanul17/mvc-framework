using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Web.Models.ViewModels;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class RoleController : BaseController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _roleService.GetAllRolesAsync();
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error loading roles");
            return View(new List<RoleViewModel>());
        }

        var viewModels = result.Data?.Select(r => new RoleViewModel
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList() ?? new List<RoleViewModel>();

        return View(viewModels);
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
            var result = await _roleService.GetRoleByIdAsync(id!);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var viewModel = new RoleViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name ?? string.Empty,
                Description = result.Data.Description,
                IsActive = result.Data.IsActive,
                CreatedAt = result.Data.CreatedAt,
                UpdatedAt = result.Data.UpdatedAt,
                IsEditMode = true
            };

            return View("CreateEdit", viewModel);
        }
        else
        {
            return View("CreateEdit", new RoleViewModel { IsEditMode = false });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("CreateEdit", model);
        }

        if (string.IsNullOrEmpty(model.Id)) // Create mode
        {
            var result = await _roleService.CreateRoleAsync(model.Name, model.Description);
            if (!result.IsSuccess)
            {
                AddErrorsToModelState(result);
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Role created successfully");
            return RedirectToAction(nameof(Index));
        }
        else // Edit mode
        {
            var result = await _roleService.UpdateRoleAsync(model.Id, model.Name, model.Description);
            if (!result.IsSuccess)
            {
                AddErrorsToModelState(result);
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Role updated successfully");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error deleting role");
        }
        else
        {
            SetSuccessMessage("Role deleted successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AssignToUser(AssignRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _roleService.AssignRoleToUserAsync(model.UserId, model.RoleId);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromUser(AssignRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _roleService.RemoveRoleFromUserAsync(model.UserId, model.RoleId);
        return HandleResult(result);
    }
}