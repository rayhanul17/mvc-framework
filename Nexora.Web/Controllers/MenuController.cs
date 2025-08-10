using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Models.ViewModels;
using System.IO;

namespace Nexora.Web.Controllers;

[Authorize]
public class MenuController : BaseController
{
    private readonly IMenuService _menuService;
    private readonly IRoleService _roleService;

    public MenuController(IMenuService menuService, IRoleService roleService)
    {
        _menuService = menuService;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _menuService.GetMenuHierarchyAsync();
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error loading menus");
            return View(new List<Menu>());
        }

        return View(result.Data);
    }

    public IActionResult Create()
    {
        return RedirectToAction(nameof(CreateEdit));
    }

    public IActionResult Edit(int id)
    {
        return RedirectToAction(nameof(CreateEdit), new { id });
    }

    public async Task<IActionResult> CreateEdit(int? id = null)
    {
        var isEditMode = id.HasValue && id.Value > 0;
        
        if (isEditMode)
        {
            var result = await _menuService.GetByIdAsync(id!.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var viewModel = new MenuViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                DisplayName = result.Data.DisplayName,
                Area = result.Data.Area,
                Controller = result.Data.Controller,
                Action = result.Data.Action,
                Url = result.Data.Url,
                Icon = result.Data.Icon,
                ParentId = result.Data.ParentId,
                Order = result.Data.Order,
                IsActive = result.Data.IsActive,
                IsEditMode = true
            };

            await PopulateViewBags();
            return View("CreateEdit", viewModel);
        }
        else
        {
            await PopulateViewBags();
            return View("CreateEdit", new MenuViewModel { IsEditMode = false });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(MenuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBags();
            return View("CreateEdit", model);
        }

        if (model.Id == 0) // Create mode
        {
            var menu = new Menu
            {
                Name = model.Name,
                DisplayName = model.DisplayName,
                Area = model.Area,
                Controller = model.Controller,
                Action = model.Action,
                Url = model.Url,
                Icon = model.Icon,
                ParentId = model.ParentId,
                Order = model.Order,
                IsActive = model.IsActive
            };

            var result = await _menuService.CreateMenuWithPermissionsAsync(menu, model.SelectedRoleIds);
            if (!result.IsSuccess)
            {
                AddErrorsToModelState(result);
                await PopulateViewBags();
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Menu created successfully");
            return RedirectToAction(nameof(Index));
        }
        else // Edit mode
        {
            var result = await _menuService.GetByIdAsync(model.Id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var menu = result.Data;
            menu.Name = model.Name;
            menu.DisplayName = model.DisplayName;
            menu.Area = model.Area;
            menu.Controller = model.Controller;
            menu.Action = model.Action;
            menu.Url = model.Url;
            menu.Icon = model.Icon;
            menu.ParentId = model.ParentId;
            menu.Order = model.Order;
            menu.IsActive = model.IsActive;

            var updateResult = await _menuService.UpdateAsync(menu);
            if (!updateResult.IsSuccess)
            {
                AddErrorsToModelState(updateResult);
                await PopulateViewBags();
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Menu updated successfully");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _menuService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error deleting menu");
        }
        else
        {
            SetSuccessMessage("Menu deleted successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetControllersAndActions()
    {
        var result = await _menuService.GetAllControllersAndActionsAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Json(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetControllersByArea(string? areaName)
    {
        var result = await _menuService.GetControllersByAreaAsync(areaName);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Json(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermission(MenuPermissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _menuService.AssignMenuToRoleAsync(
            model.MenuId, 
            model.RoleId, 
            model.CanView, 
            model.CanCreate, 
            model.CanEdit, 
            model.CanDelete
        );

        return HandleResult(result);
    }

    private async Task PopulateViewBags()
    {
        var menusResult = await _menuService.GetAllAsync();
        var rolesResult = await _roleService.GetAllRolesAsync();
        var controllersResult = await _menuService.GetAllControllersAndActionsAsync();

        ViewBag.ParentMenus = new SelectList(
            menusResult.Data?.Where(m => m.ParentId == null) ?? Enumerable.Empty<Menu>(),
            "Id",
            "Name"
        );

        ViewBag.Roles = rolesResult.Data ?? Enumerable.Empty<ApplicationRole>();
        ViewBag.Controllers = controllersResult.Data ?? new Dictionary<string, List<string>>();
        
        // Get all areas from the Areas folder
        var areas = new List<string>();
        var areasPath = Path.Combine(Directory.GetCurrentDirectory(), "Areas");
        if (Directory.Exists(areasPath))
        {
            areas = Directory.GetDirectories(areasPath)
                .Select(d => new DirectoryInfo(d).Name)
                .ToList();
        }
        ViewBag.Areas = areas;
    }
}