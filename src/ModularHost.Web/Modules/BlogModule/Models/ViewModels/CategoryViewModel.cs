using System;
using System.ComponentModel.DataAnnotations;

namespace MRCMS.Modules.Blog.Models.ViewModels
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public required string Name { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Slug")]
        public string? Slug { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}