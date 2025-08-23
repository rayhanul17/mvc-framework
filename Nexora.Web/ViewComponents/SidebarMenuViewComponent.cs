using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.ViewComponents;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IMenuService _menuService;
    private readonly IPermissionService _permissionService;

    public SidebarMenuViewComponent(IMenuService menuService, IPermissionService permissionService)
    {
        _menuService = menuService;
        _permissionService = permissionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? currentController, string? currentAction, string? currentArea)
    {
        IEnumerable<Core.Entities.Menu> menus;
        
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // Anonymous user - only show AllowAnonymous menus
            var anonymousResult = await _menuService.GetAnonymousMenusAsync();
            menus = anonymousResult.IsSuccess ? anonymousResult.Data : new List<Core.Entities.Menu>();
        }
        else
        {
            var userId = (User as System.Security.Claims.ClaimsPrincipal)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                // Authenticated but no user ID - show authenticated menus only
                var authenticatedResult = await _menuService.GetAuthenticatedMenusAsync();
                menus = authenticatedResult.IsSuccess ? authenticatedResult.Data : new List<Core.Entities.Menu>();
            }
            else
            {
                // Authenticated with user ID - show menus based on permissions with inheritance
                var userResult = await _menuService.GetUserMenusAsync(userId);
                menus = userResult.IsSuccess ? userResult.Data : new List<Core.Entities.Menu>();
                
                // Apply permission inheritance for child menus
                menus = await ApplyPermissionInheritanceAsync(menus, (ClaimsPrincipal)User);
            }
        }
        
        return View(new SidebarMenuViewModel 
        { 
            Menus = menus,
            CurrentController = currentController,
            CurrentAction = currentAction,
            CurrentArea = currentArea,
            CurrentPath = HttpContext.Request.Path
        });
    }

    /// <summary>
    /// Apply permission inheritance for child menus.
    /// If a user has permission to access a parent menu but not a specific child menu,
    /// the child menu will inherit permission from the parent if it's in the same area.
    /// </summary>
    private async Task<IEnumerable<Core.Entities.Menu>> ApplyPermissionInheritanceAsync(
        IEnumerable<Core.Entities.Menu> menus, 
        ClaimsPrincipal user)
    {
        var menuList = menus.ToList();
        var resultMenus = new List<Core.Entities.Menu>();

        // Check if user is SuperAdmin (has access to everything)
        if (await _permissionService.IsSuperAdminAsync(user))
        {
            return menuList; // SuperAdmin sees all menus
        }

        foreach (var menu in menuList.Where(m => m.ParentId == null))
        {
            var processedMenu = await ProcessMenuWithInheritanceAsync(menu, user);
            if (processedMenu != null)
            {
                resultMenus.Add(processedMenu);
            }
        }

        return resultMenus;
    }

    /// <summary>
    /// Process a menu item and its children with permission inheritance
    /// </summary>
    private async Task<Core.Entities.Menu?> ProcessMenuWithInheritanceAsync(
        Core.Entities.Menu menu, 
        ClaimsPrincipal user)
    {
        // Check if user has permission for this menu
        bool hasMenuPermission = await HasMenuPermissionAsync(menu, user);
        
        // Process children
        var filteredChildren = new List<Core.Entities.Menu>();
        if (menu.Children != null && menu.Children.Any())
        {
            foreach (var child in menu.Children)
            {
                var processedChild = await ProcessChildMenuWithInheritanceAsync(child, menu, user, hasMenuPermission);
                if (processedChild != null)
                {
                    filteredChildren.Add(processedChild);
                }
            }
        }

        // If menu has permission OR has accessible children, include it
        if (hasMenuPermission || filteredChildren.Any())
        {
            var resultMenu = CloneMenu(menu);
            resultMenu.Children = filteredChildren.Any() ? filteredChildren : null;
            return resultMenu;
        }

        return null;
    }

    /// <summary>
    /// Process child menu with inheritance logic
    /// </summary>
    private async Task<Core.Entities.Menu?> ProcessChildMenuWithInheritanceAsync(
        Core.Entities.Menu childMenu, 
        Core.Entities.Menu parentMenu,
        ClaimsPrincipal user,
        bool parentHasPermission)
    {
        // Check if user has direct permission for this child menu
        bool hasDirectPermission = await HasMenuPermissionAsync(childMenu, user);

        // Check if child can inherit permission from parent
        bool canInherit = CanInheritFromParent(childMenu, parentMenu);
        bool hasInheritedPermission = parentHasPermission && canInherit;

        // Child menu is accessible if it has direct permission OR inherited permission
        if (hasDirectPermission || hasInheritedPermission)
        {
            return CloneMenu(childMenu);
        }

        return null;
    }

    /// <summary>
    /// Check if user has permission to access a specific menu
    /// </summary>
    private async Task<bool> HasMenuPermissionAsync(Core.Entities.Menu menu, ClaimsPrincipal user)
    {
        // If menu allows anonymous access, return true
        if (menu.AllowAnonymous)
        {
            return true;
        }

        // If menu doesn't require authentication, return true for authenticated users
        if (!menu.RequireAuthentication && user.Identity?.IsAuthenticated == true)
        {
            return true;
        }

        // If menu has controller/action, check permission
        if (!string.IsNullOrEmpty(menu.Controller))
        {
            var area = menu.Area ?? "";
            var action = menu.Action ?? "Index";
            return await _permissionService.HasPermissionAsync(user, area, menu.Controller, action);
        }

        // If menu has URL but no controller, check if user has any permission in the area
        if (!string.IsNullOrEmpty(menu.Area))
        {
            return await _permissionService.UserHasAnyPermissionInAreaAsync(user, menu.Area);
        }

        // Default to true for authenticated users if no specific permission check can be made
        return user.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Determine if a child menu can inherit permissions from its parent
    /// </summary>
    private bool CanInheritFromParent(Core.Entities.Menu childMenu, Core.Entities.Menu parentMenu)
    {
        // Child can inherit if:
        // 1. Both are in the same area, OR
        // 2. Child doesn't specify an area and parent has an area (inherit parent's area context), OR
        // 3. Both don't have areas (root level inheritance)
        
        var childArea = childMenu.Area ?? "";
        var parentArea = parentMenu.Area ?? "";

        // Same area or both empty areas
        if (childArea.Equals(parentArea, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Child has no area but parent does (inherit area context)
        if (string.IsNullOrEmpty(childArea) && !string.IsNullOrEmpty(parentArea))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clone a menu item (shallow copy for display purposes)
    /// </summary>
    private Core.Entities.Menu CloneMenu(Core.Entities.Menu original)
    {
        return new Core.Entities.Menu
        {
            Id = original.Id,
            Name = original.Name,
            DisplayName = original.DisplayName,
            Controller = original.Controller,
            Action = original.Action,
            Area = original.Area,
            Url = original.Url,
            ActiveMenuUrl = original.ActiveMenuUrl,
            Icon = original.Icon,
            Order = original.Order,
            IsActive = original.IsActive,
            AllowAnonymous = original.AllowAnonymous,
            RequireAuthentication = original.RequireAuthentication,
            ParentId = original.ParentId,
            Parent = original.Parent,
            Children = original.Children // Will be set separately in processing
        };
    }
}

public class SidebarMenuViewModel
{
    public IEnumerable<Core.Entities.Menu> Menus { get; set; } = new List<Core.Entities.Menu>();
    public string? CurrentController { get; set; }
    public string? CurrentAction { get; set; }
    public string? CurrentArea { get; set; }
    public string? CurrentPath { get; set; }
    
    public bool IsMenuActive(Core.Entities.Menu menu)
    {
        // Handle exact path matching first
        if (!string.IsNullOrEmpty(CurrentPath))
        {
            // Check ActiveMenuUrl if it's set
            if (!string.IsNullOrEmpty(menu.ActiveMenuUrl))
            {
                // Special handling for root path "/"
                if (menu.ActiveMenuUrl == "/" && CurrentPath == "/")
                {
                    return true;
                }
                // For non-root paths, use StartsWith but ensure it's not the root "/"
                else if (menu.ActiveMenuUrl != "/" && CurrentPath.StartsWith(menu.ActiveMenuUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // Make sure it's an exact match or the next character is a slash to avoid partial matches
                    return CurrentPath.Length == menu.ActiveMenuUrl.Length || 
                           CurrentPath[menu.ActiveMenuUrl.Length] == '/' ||
                           CurrentPath[menu.ActiveMenuUrl.Length] == '?';
                }
            }
            
            // Fallback to checking Url if ActiveMenuUrl didn't match
            if (!string.IsNullOrEmpty(menu.Url))
            {
                // Special handling for root path "/"
                if (menu.Url == "/" && CurrentPath == "/")
                {
                    return true;
                }
                // For non-root paths, use StartsWith but ensure it's not the root "/"
                else if (menu.Url != "/" && CurrentPath.StartsWith(menu.Url, StringComparison.OrdinalIgnoreCase))
                {
                    // Make sure it's an exact match or the next character is a slash to avoid partial matches
                    return CurrentPath.Length == menu.Url.Length || 
                           CurrentPath[menu.Url.Length] == '/' ||
                           CurrentPath[menu.Url.Length] == '?';
                }
            }
        }
        
        // Finally, check controller/action/area matching
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