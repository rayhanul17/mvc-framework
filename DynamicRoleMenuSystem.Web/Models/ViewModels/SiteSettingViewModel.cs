using System.ComponentModel.DataAnnotations;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Web.Models.ViewModels;

public class SiteSettingViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Key is required")]
    [Display(Name = "Setting Key")]
    public string Key { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Value is required")]
    [Display(Name = "Setting Value")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public SettingCategory Category { get; set; }
    
    [Required]
    [Display(Name = "Type")]
    public SettingType Type { get; set; }
    
    [Display(Name = "Valid Values (JSON)")]
    public string? ValidValues { get; set; }
    
    [Display(Name = "Is Required")]
    public bool IsRequired { get; set; }
    
    [Display(Name = "Is System Setting")]
    public bool IsSystemSetting { get; set; }
    
    [Display(Name = "Display Order")]
    public int Order { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CreateSiteSettingViewModel
{
    [Required(ErrorMessage = "Key is required")]
    [Display(Name = "Setting Key")]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9._-]*$", ErrorMessage = "Key must start with a letter and contain only letters, numbers, dots, underscores, and hyphens")]
    public string Key { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Value is required")]
    [Display(Name = "Setting Value")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public SettingCategory Category { get; set; }
    
    [Required]
    [Display(Name = "Type")]
    public SettingType Type { get; set; }
    
    [Display(Name = "Valid Values (JSON)")]
    public string? ValidValues { get; set; }
    
    [Display(Name = "Is Required")]
    public bool IsRequired { get; set; }
    
    [Display(Name = "Is System Setting")]
    public bool IsSystemSetting { get; set; }
    
    [Display(Name = "Display Order")]
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
    public int Order { get; set; }
}

public class EditSiteSettingViewModel
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Key is required")]
    [Display(Name = "Setting Key")]
    public string Key { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Value is required")]
    [Display(Name = "Setting Value")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public SettingCategory Category { get; set; }
    
    [Required]
    [Display(Name = "Type")]
    public SettingType Type { get; set; }
    
    [Display(Name = "Valid Values (JSON)")]
    public string? ValidValues { get; set; }
    
    [Display(Name = "Is Required")]
    public bool IsRequired { get; set; }
    
    [Display(Name = "Is System Setting")]
    public bool IsSystemSetting { get; set; }
    
    [Display(Name = "Display Order")]
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
    public int Order { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class SiteSettingCategoryViewModel
{
    public SettingCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<SiteSettingViewModel> Settings { get; set; } = new();
}

public class BulkUpdateSettingsViewModel
{
    public List<BulkSettingItem> Settings { get; set; } = new();
}

public class BulkSettingItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingType Type { get; set; }
    public string? ValidValues { get; set; }
    public bool IsRequired { get; set; }
}

public class SiteSettingCreateEditViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Key is required")]
    [Display(Name = "Setting Key")]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9._-]*$", ErrorMessage = "Key must start with a letter and contain only letters, numbers, dots, underscores, and hyphens")]
    public string Key { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Value is required")]
    [Display(Name = "Setting Value")]
    public string Value { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public SettingCategory Category { get; set; }
    
    [Required]
    [Display(Name = "Type")]
    public SettingType Type { get; set; }
    
    [Display(Name = "Valid Values (JSON)")]
    public string? ValidValues { get; set; }
    
    [Display(Name = "Is Required")]
    public bool IsRequired { get; set; }
    
    [Display(Name = "Is System Setting")]
    public bool IsSystemSetting { get; set; }
    
    [Display(Name = "Display Order")]
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
    public int Order { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsEditMode { get; set; }
}