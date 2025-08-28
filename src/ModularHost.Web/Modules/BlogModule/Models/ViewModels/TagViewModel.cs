using System;
using System.ComponentModel.DataAnnotations;

namespace ModularHost.Web.Modules.Blog.Models.ViewModels
{
    public class TagViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        [Display(Name = "Tag Name")]
        public required string Name { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Slug")]
        public string? Slug { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}