using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models.ViewModels;

public class BlogPostViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(300, ErrorMessage = "Slug cannot exceed 300 characters")]
    [Display(Name = "URL Slug")]
    public string? Slug { get; set; }
    
    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    [Display(Name = "Summary")]
    public string? Summary { get; set; }
    
    [Required(ErrorMessage = "Content is required")]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Featured image URL cannot exceed 500 characters")]
    [Display(Name = "Featured Image URL")]
    public string? FeaturedImageUrl { get; set; }
    
    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    
    [Display(Name = "Publish Now")]
    public bool IsPublished { get; set; }
    
    [Display(Name = "Published Date")]
    public DateTime? PublishedDate { get; set; }
    
    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    [Display(Name = "Tags (comma separated)")]
    public string? Tags { get; set; }
    
    [StringLength(200, ErrorMessage = "Meta title cannot exceed 200 characters")]
    [Display(Name = "Meta Title (SEO)")]
    public string? MetaTitle { get; set; }
    
    [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
    [Display(Name = "Meta Description (SEO)")]
    public string? MetaDescription { get; set; }
    
    [StringLength(500, ErrorMessage = "Meta keywords cannot exceed 500 characters")]
    [Display(Name = "Meta Keywords (SEO)")]
    public string? MetaKeywords { get; set; }
    
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public int ViewCount { get; set; }
    
    [Display(Name = "Enable Comments")]
    public bool CommentsEnabled { get; set; } = true;
    
    [Display(Name = "Require Comment Approval")]
    public bool RequireCommentApproval { get; set; } = false;
}