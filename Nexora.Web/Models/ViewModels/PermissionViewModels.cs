using Nexora.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nexora.Web.Models.ViewModels;

public class RolePermissionViewModel
{
    public ApplicationRole Role { get; set; } = null!;
    public List<Permission> Permissions { get; set; } = new List<Permission>();
}

public class UserPermissionViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<string> Roles { get; set; } = new List<string>();
    public List<Permission> Permissions { get; set; } = new List<Permission>();
}

public class PermissionCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public List<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Controllers { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Actions { get; set; } = new List<SelectListItem>();
}

public class PermissionEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class PermissionTestDashboardViewModel
{
    public List<UserTestInfo> Users { get; set; } = new List<UserTestInfo>();
    public Dictionary<string, List<Core.Entities.Permission>> Permissions { get; set; } = new Dictionary<string, List<Core.Entities.Permission>>();
}

public class UserTestInfo
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class PermissionTestRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class BulkPermissionTestRequest
{
    public List<string> UserIds { get; set; } = new List<string>();
    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}