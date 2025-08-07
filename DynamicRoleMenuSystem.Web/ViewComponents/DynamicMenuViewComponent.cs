using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Application.Interfaces;

namespace DynamicRoleMenuSystem.Web.ViewComponents;

public class DynamicMenuViewComponent : ViewComponent
{
    private readonly IMenuService _menuService;

    public DynamicMenuViewComponent(IMenuService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View(new List<Core.Entities.Menu>());
        }

        var userId = UserClaimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return View(new List<Core.Entities.Menu>());
        }

        var result = await _menuService.GetUserMenusAsync(userId);
        
        if (!result.IsSuccess)
        {
            return View(new List<Core.Entities.Menu>());
        }

        return View(result.Data);
    }
}