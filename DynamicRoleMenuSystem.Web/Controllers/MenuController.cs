using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Models.ViewModels;

namespace DynamicRoleMenuSystem.Web.Controllers;

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

    public async Task<IActionResult> Create()
    {
        await PopulateViewBags();
        return View(new MenuViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBags();
            return View(model);
        }

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
            return View(model);
        }

        SetSuccessMessage("Menu created successfully");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _menuService.GetByIdAsync(id);
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
            IsActive = result.Data.IsActive
        };

        await PopulateViewBags();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateViewBags();
            return View(model);
        }

        var result = await _menuService.GetByIdAsync(id);
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
            return View(model);
        }

        SetSuccessMessage("Menu updated successfully");
        return RedirectToAction(nameof(Index));
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
    }
}