using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;

namespace Nexora.Web.ViewComponents;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IMenuService _menuService;

    public SidebarMenuViewComponent(IMenuService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? currentController, string? currentAction, string? currentArea)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View(new SidebarMenuViewModel 
            { 
                Menus = new List<Core.Entities.Menu>(),
                CurrentController = currentController,
                CurrentAction = currentAction,
                CurrentArea = currentArea
            });
        }

        var userId = (User as System.Security.Claims.ClaimsPrincipal)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return View(new SidebarMenuViewModel 
            { 
                Menus = new List<Core.Entities.Menu>(),
                CurrentController = currentController,
                CurrentAction = currentAction,
                CurrentArea = currentArea
            });
        }

        var result = await _menuService.GetUserMenusAsync(userId);
        
        var menus = result.IsSuccess ? result.Data : new List<Core.Entities.Menu>();
        
        return View(new SidebarMenuViewModel 
        { 
            Menus = menus,
            CurrentController = currentController,
            CurrentAction = currentAction,
            CurrentArea = currentArea
        });
    }
}

public class SidebarMenuViewModel
{
    public IEnumerable<Core.Entities.Menu> Menus { get; set; } = new List<Core.Entities.Menu>();
    public string? CurrentController { get; set; }
    public string? CurrentAction { get; set; }
    public string? CurrentArea { get; set; }
    
    public bool IsMenuActive(Core.Entities.Menu menu)
    {
        if (!string.IsNullOrEmpty(menu.Controller))
        {
            var controllerMatch = string.Equals(menu.Controller, CurrentController, StringComparison.OrdinalIgnoreCase);
            var actionMatch = string.IsNullOrEmpty(menu.Action) || 
                            string.Equals(menu.Action, CurrentAction, StringComparison.OrdinalIgnoreCase);
            var areaMatch = string.IsNullOrEmpty(menu.Area) || 
                          string.Equals(menu.Area, CurrentArea, StringComparison.OrdinalIgnoreCase);
            
            return controllerMatch && actionMatch && areaMatch;
        }
        
        return false;
    }
    
    public bool HasActiveChild(Core.Entities.Menu menu)
    {
        if (menu.Children == null || !menu.Children.Any())
            return false;
            
        return menu.Children.Any(child => IsMenuActive(child));
    }
}