using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Application.Interfaces;

public interface IMenuService : IBaseService<Menu>
{
    Task<Result<IEnumerable<Menu>>> GetMenusByRoleIdAsync(string roleId);
    Task<Result<IEnumerable<Menu>>> GetMenuHierarchyAsync();
    Task<Result<IEnumerable<Menu>>> GetUserMenusAsync(string userId);
    Task<Result<Menu>> CreateMenuWithPermissionsAsync(Menu menu, List<string> roleIds);
    Task<Result> AssignMenuToRoleAsync(int menuId, string roleId, bool canView = true, bool canCreate = false, bool canEdit = false, bool canDelete = false);
    Task<Result> RemoveMenuFromRoleAsync(int menuId, string roleId);
    Task<Result<Dictionary<string, List<string>>>> GetAllControllersAndActionsAsync();
}