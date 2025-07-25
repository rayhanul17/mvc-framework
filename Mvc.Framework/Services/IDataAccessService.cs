﻿using mvc.framework.Models;
using System.Security.Claims;

namespace mvc.framework.Services
{
	public interface IDataAccessService
	{
		Task<bool> GetMenuItemsAsync(ClaimsPrincipal ctx, string? ctrl, string? act);
		Task<List<NavigationMenuViewModel>> GetMenuItemsAsync(ClaimsPrincipal principal);
		Task<List<NavigationMenuViewModel>> GetPermissionsByRoleIdAsync(string? id);
		Task<bool> SetPermissionsByRoleIdAsync(string? id, IEnumerable<Guid> permissionIds);
	}
}