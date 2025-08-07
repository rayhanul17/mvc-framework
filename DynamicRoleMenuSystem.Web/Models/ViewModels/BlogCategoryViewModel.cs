using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Web.Models.ViewModels;

public class BlogCategoryViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
    [Display(Name = "URL Slug")]
    public string? Slug { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Display order is required")]
    [Display(Name = "Display Order")]
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    public int DisplayOrder { get; set; }
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}