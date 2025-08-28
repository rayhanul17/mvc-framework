using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class MenuService : BaseService<Menu>, IMenuService
{
    public MenuService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<IEnumerable<Menu>>> GetMenusByRoleIdAsync(string roleId)
    {
        try
        {
            var menus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Include(m => m.RoleMenus)
                .Where(m => m.RoleMenus.Any(rm => rm.RoleId == roleId && rm.CanView))
                .OrderBy(m => m.Order)
                .ToListAsync();

            return Result<IEnumerable<Menu>>.Success(menus);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving menus for role: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Menu>>> GetMenuHierarchyAsync()
    {
        try
        {
            var menus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Where(m => m.ParentId == null && m.IsActive)
                .OrderBy(m => m.Order)
                .ToListAsync();

            return Result<IEnumerable<Menu>>.Success(menus);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving menu hierarchy: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Menu>>> GetAnonymousMenusAsync()
    {
        try
        {
            var anonymousMenus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Where(m => m.IsActive && m.AllowAnonymous)
                .OrderBy(m => m.Order)
                .ToListAsync();
                
            var menuHierarchy = BuildMenuHierarchy(anonymousMenus);
            return Result<IEnumerable<Menu>>.Success(menuHierarchy);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving anonymous menus: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<Menu>>> GetAuthenticatedMenusAsync()
    {
        try
        {
            var authenticatedMenus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Where(m => m.IsActive && (m.AllowAnonymous || m.RequireAuthentication))
                .OrderBy(m => m.Order)
                .ToListAsync();
                
            var menuHierarchy = BuildMenuHierarchy(authenticatedMenus);
            return Result<IEnumerable<Menu>>.Success(menuHierarchy);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving authenticated menus: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Menu>>> GetUserMenusAsync(string userId)
    {
        try
        {
            // Check if user is SuperAdmin - use AsNoTracking for fresh data
            var user = await _unitOfWork.Repository<ApplicationUser>()
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user != null && user.IsSuperAdmin)
            {
                // SuperAdmin gets all active menus - fetch all menus first
                var allSuperAdminMenus = await _unitOfWork.Repository<Menu>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Order)
                    .ToListAsync();
                
                // Build proper hierarchy
                var superAdminMenuHierarchy = BuildMenuHierarchy(allSuperAdminMenus);
                return Result<IEnumerable<Menu>>.Success(superAdminMenuHierarchy);
            }
            
            // Get user's active roles
            var now = DateTime.UtcNow;
            var userRoles = await _unitOfWork.Repository<UserRole>()
                .GetQueryable()
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && 
                            ur.IsActive && 
                            (ur.ExpiresAt == null || ur.ExpiresAt > now))
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Get all active menus
            var allMenus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Include(m => m.RoleMenus)
                .Where(m => m.IsActive)
                .ToListAsync();

            // Filter menus based on permissions
            var visibleMenus = new List<Menu>();
            
            foreach (var menu in allMenus)
            {
                bool shouldInclude = false;
                
                // Check if menu allows anonymous access
                if (menu.AllowAnonymous)
                {
                    shouldInclude = true;
                }
                // Check if menu requires authentication only
                else if (menu.RequireAuthentication && user != null)
                {
                    shouldInclude = true;
                }
                // Check if user has permission through roles
                else if (userRoles.Any() && menu.RoleMenus.Any(rm => userRoles.Contains(rm.RoleId) && rm.CanView))
                {
                    shouldInclude = true;
                }
                // Check if user has permission through controller/action permissions
                else if (!string.IsNullOrEmpty(menu.Controller) && !string.IsNullOrEmpty(menu.Action))
                {
                    shouldInclude = await CheckUserHasPermissionForMenuAsync(userId, userRoles, menu.Area, menu.Controller, menu.Action);
                }
                
                if (shouldInclude)
                {
                    visibleMenus.Add(menu);
                }
            }

            var menuHierarchy = BuildMenuHierarchy(visibleMenus);
            return Result<IEnumerable<Menu>>.Success(menuHierarchy);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving user menus: {ex.Message}");
        }
    }
    
    private async Task<bool> CheckUserHasPermissionForMenuAsync(string userId, List<string> userRoleIds, string? area, string controller, string action)
    {
        if (!userRoleIds.Any())
            return false;
            
        // Check if any of user's roles have permission for this action
        var hasPermission = await _unitOfWork.Repository<RolePermission>()
            .GetQueryable()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                        rp.IsActive && 
                        rp.Permission.IsActive)
            .AnyAsync(rp => 
                (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                rp.Permission.Controller == controller &&
                rp.Permission.Action == action);

        // If no exact match, check for wildcard action permission
        if (!hasPermission)
        {
            hasPermission = await _unitOfWork.Repository<RolePermission>()
                .GetQueryable()
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    (string.IsNullOrEmpty(rp.Permission.Area) || rp.Permission.Area == area) &&
                    rp.Permission.Controller == controller &&
                    rp.Permission.Action == "*");
        }

        // If still no match, check for area-wide permission
        if (!hasPermission && !string.IsNullOrEmpty(area))
        {
            hasPermission = await _unitOfWork.Repository<RolePermission>()
                .GetQueryable()
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && 
                            rp.IsActive && 
                            rp.Permission.IsActive)
                .AnyAsync(rp => 
                    rp.Permission.Area == area &&
                    rp.Permission.Controller == "*" &&
                    rp.Permission.Action == "*");
        }

        return hasPermission;
    }

    public async Task<Result<Menu>> CreateMenuWithPermissionsAsync(Menu menu, List<string> roleIds)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            await _unitOfWork.Repository<Menu>().AddAsync(menu);
            await _unitOfWork.SaveChangesAsync();

            if (roleIds != null && roleIds.Any())
            {
                var roleMenus = roleIds.Select(roleId => new RoleMenu
                {
                    MenuId = menu.Id,
                    RoleId = roleId,
                    CanView = true,
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false
                }).ToList();

                await _unitOfWork.Repository<RoleMenu>().AddRangeAsync(roleMenus);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();
            return Result<Menu>.Success(menu);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Result<Menu>.Failure($"Error creating menu with permissions: {ex.Message}");
        }
    }

    public async Task<Result> AssignMenuToRoleAsync(int menuId, string roleId, bool canView = true, bool canCreate = false, bool canEdit = false, bool canDelete = false)
    {
        try
        {
            var existingRoleMenu = await _unitOfWork.Repository<RoleMenu>()
                .FirstOrDefaultAsync(rm => rm.MenuId == menuId && rm.RoleId == roleId);

            if (existingRoleMenu != null)
            {
                existingRoleMenu.CanView = canView;
                existingRoleMenu.CanCreate = canCreate;
                existingRoleMenu.CanEdit = canEdit;
                existingRoleMenu.CanDelete = canDelete;
                _unitOfWork.Repository<RoleMenu>().Update(existingRoleMenu);
            }
            else
            {
                var roleMenu = new RoleMenu
                {
                    MenuId = menuId,
                    RoleId = roleId,
                    CanView = canView,
                    CanCreate = canCreate,
                    CanEdit = canEdit,
                    CanDelete = canDelete
                };
                await _unitOfWork.Repository<RoleMenu>().AddAsync(roleMenu);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning menu to role: {ex.Message}");
        }
    }

    public async Task<Result> RemoveMenuFromRoleAsync(int menuId, string roleId)
    {
        try
        {
            var roleMenu = await _unitOfWork.Repository<RoleMenu>()
                .FirstOrDefaultAsync(rm => rm.MenuId == menuId && rm.RoleId == roleId);

            if (roleMenu != null)
            {
                _unitOfWork.Repository<RoleMenu>().Remove(roleMenu);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing menu from role: {ex.Message}");
        }
    }

    public Task<Result<Dictionary<string, List<string>>>> GetAllControllersAndActionsAsync()
    {
        try
        {
            var controllers = new Dictionary<string, List<string>>();
            var assembly = Assembly.GetEntryAssembly();
            
            if (assembly == null)
                return Task.FromResult(Result<Dictionary<string, List<string>>>.Success(controllers));

            var controllerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ControllerBase)))
                .ToList();

            foreach (var controllerType in controllerTypes)
            {
                var controllerName = controllerType.Name.Replace("Controller", "");
                var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName && m.IsPublic && m.DeclaringType == controllerType)
                    .Where(m => m.GetCustomAttribute<NonActionAttribute>() == null)
                    .Select(m => m.Name)
                    .Distinct()
                    .ToList();

                if (actions.Any())
                {
                    controllers[controllerName] = actions;
                }
            }

            return Task.FromResult(Result<Dictionary<string, List<string>>>.Success(controllers));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<Dictionary<string, List<string>>>.Failure($"Error getting controllers and actions: {ex.Message}"));
        }
    }

    public Task<Result<Dictionary<string, List<string>>>> GetControllersByAreaAsync(string? areaName)
    {
        try
        {
            var controllers = new Dictionary<string, List<string>>();
            var assembly = Assembly.GetEntryAssembly();
            
            if (assembly == null)
                return Task.FromResult(Result<Dictionary<string, List<string>>>.Success(controllers));

            var controllerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ControllerBase)))
                .ToList();

            foreach (var controllerType in controllerTypes)
            {
                // Check if controller belongs to the specified area
                var areaAttribute = controllerType.GetCustomAttribute<AreaAttribute>();
                var controllerAreaName = areaAttribute?.RouteValue;
                
                // Filter based on area
                if (string.IsNullOrEmpty(areaName))
                {
                    // If no area specified, get controllers without area attribute
                    if (areaAttribute != null)
                        continue;
                }
                else
                {
                    // If area specified, get controllers with matching area attribute
                    if (controllerAreaName != areaName)
                        continue;
                }

                var controllerName = controllerType.Name.Replace("Controller", "");
                var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName && m.IsPublic && m.DeclaringType == controllerType)
                    .Where(m => m.GetCustomAttribute<NonActionAttribute>() == null)
                    .Select(m => m.Name)
                    .Distinct()
                    .ToList();

                if (actions.Any())
                {
                    controllers[controllerName] = actions;
                }
            }

            return Task.FromResult(Result<Dictionary<string, List<string>>>.Success(controllers));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<Dictionary<string, List<string>>>.Failure($"Error getting controllers by area: {ex.Message}"));
        }
    }

    private IEnumerable<Menu> BuildMenuHierarchy(IEnumerable<Menu> allMenus)
    {
        var menuList = allMenus.ToList();
        var menuDict = menuList.ToDictionary(m => m.Id);
        var rootMenus = new List<Menu>();
        
        // Initialize all Children collections
        foreach (var menu in menuList)
        {
            if (menu.Children == null)
            {
                menu.Children = new List<Menu>();
            }
        }

        foreach (var menu in menuList)
        {
            if (menu.ParentId == null)
            {
                rootMenus.Add(menu);
            }
            else if (menuDict.ContainsKey(menu.ParentId.Value))
            {
                var parent = menuDict[menu.ParentId.Value];
                if (!parent.Children.Any(c => c.Id == menu.Id))
                {
                    parent.Children.Add(menu);
                }
            }
        }

        return rootMenus.OrderBy(m => m.Order);
    }
}