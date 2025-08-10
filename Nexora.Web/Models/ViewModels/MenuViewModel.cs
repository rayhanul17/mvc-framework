using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models.ViewModels;

public class MenuViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Menu name is required")]
    [Display(Name = "Menu Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Display Name")]
    public string? DisplayName { get; set; }
    
    [Display(Name = "Area")]
    public string? Area { get; set; }
    
    [Display(Name = "Controller")]
    public string? Controller { get; set; }
    
    [Display(Name = "Action")]
    public string? Action { get; set; }
    
    [Display(Name = "URL")]
    public string? Url { get; set; }
    
    [Display(Name = "Icon")]
    public string? Icon { get; set; }
    
    [Display(Name = "Parent Menu")]
    public int? ParentId { get; set; }
    
    [Display(Name = "Order")]
    public int Order { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string> SelectedRoleIds { get; set; } = new List<string>();
    public bool IsEditMode { get; set; }
}

public class MenuPermissionViewModel
{
    [Required]
    public int MenuId { get; set; }
    
    [Required]
    public string RoleId { get; set; } = string.Empty;
    
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    
    public string? MenuName { get; set; }
    public string? RoleName { get; set; }
}