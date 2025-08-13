using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nexora.Web.Areas.CustomerService.Models;

public class RoleMappingViewModel
{
    public List<RoleMappingItem> Mappings { get; set; } = new();
    public List<SelectListItem> AvailableRoles { get; set; } = new();
}

public class RoleMappingItem
{
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Customer Service Role")]
    public string CustomerServiceRole { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Mapped ASP.NET Role")]
    public string AspNetRoleName { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemDefault { get; set; } = false;
}

public class SaveRoleMappingModel
{
    [Required]
    public string CustomerServiceRole { get; set; } = string.Empty;
    
    [Required]
    public string AspNetRoleName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}