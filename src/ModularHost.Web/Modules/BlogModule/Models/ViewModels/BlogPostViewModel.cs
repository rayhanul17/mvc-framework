using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MRCMS.Modules.Blog.Models.ViewModels
{
    public class BlogPostViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        [Display(Name = "Title")]
        public required string Title { get; set; }
        
        [StringLength(255)]
        [Display(Name = "Slug (URL)")]
        public string? Slug { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Summary")]
        public string? Summary { get; set; }
        
        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public required string Content { get; set; }
        
        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImageFile { get; set; }
        public string? FeaturedImage { get; set; }
        
        [Display(Name = "Category")]
        public Guid? CategoryId { get; set; }
        
        [Display(Name = "Tags")]
        public List<Guid> SelectedTagIds { get; set; } = new List<Guid>();
        
        [Display(Name = "Publish")]
        public bool IsPublished { get; set; }
        
        [Display(Name = "Publish Date")]
        public DateTime? PublishedAt { get; set; }
        
        [StringLength(255)]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Meta Keywords")]
        public string? MetaKeywords { get; set; }
        
        // For dropdowns
        public SelectList? Categories { get; set; }
        public MultiSelectList? Tags { get; set; }
    }
}