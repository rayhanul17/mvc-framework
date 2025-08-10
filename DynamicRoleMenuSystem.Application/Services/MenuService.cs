using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Core.Interfaces;

namespace DynamicRoleMenuSystem.Application.Services;

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
                // SuperAdmin gets all active menus - use AsNoTracking for fresh data
                var allMenus = await _unitOfWork.Repository<Menu>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(m => m.Children)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Order)
                    .ToListAsync();
                
                var allMenuHierarchy = BuildMenuHierarchy(allMenus);
                return Result<IEnumerable<Menu>>.Success(allMenuHierarchy);
            }
            
            // Regular users get menus based on their roles - use AsNoTracking for fresh data
            var userRoles = await _unitOfWork.Repository<UserRole>()
                .GetQueryable()
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var menus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .AsNoTracking()
                .Include(m => m.Children)
                .Include(m => m.RoleMenus)
                .Where(m => m.RoleMenus.Any(rm => userRoles.Contains(rm.RoleId) && rm.CanView) && m.IsActive)
                .Distinct()
                .OrderBy(m => m.Order)
                .ToListAsync();

            var menuHierarchy = BuildMenuHierarchy(menus);
            return Result<IEnumerable<Menu>>.Success(menuHierarchy);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Menu>>.Failure($"Error retrieving user menus: {ex.Message}");
        }
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

    private IEnumerable<Menu> BuildMenuHierarchy(IEnumerable<Menu> allMenus)
    {
        var menuDict = allMenus.ToDictionary(m => m.Id);
        var rootMenus = new List<Menu>();

        foreach (var menu in allMenus)
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