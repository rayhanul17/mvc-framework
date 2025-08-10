using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models.ViewModels;

public class RoleViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Role name is required")]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEditMode { get; set; }
}

public class AssignRoleViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string RoleId { get; set; } = string.Empty;
    
    public string? UserName { get; set; }
    public string? RoleName { get; set; }
}