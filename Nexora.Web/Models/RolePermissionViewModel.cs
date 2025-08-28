using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models;

public class RolePermissionViewModel
{
    public string RoleId { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Display(Name = "Permission URLs (comma-separated)")]
    public string? PermissionUrls { get; set; }
    
    public List<string> CurrentPermissions { get; set; } = new List<string>();
}