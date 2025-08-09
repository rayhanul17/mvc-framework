using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class BlogPost : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(300)]
    public string? Slug { get; set; }
    
    [StringLength(500)]
    public string? Summary { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? FeaturedImageUrl { get; set; }
    
    public int CategoryId { get; set; }
    
    public bool IsPublished { get; set; } = false;
    
    public DateTime? PublishedDate { get; set; }
    
    public int ViewCount { get; set; } = 0;
    
    [StringLength(500)]
    public string? Tags { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(500)]
    public string? MetaKeywords { get; set; }
    
    public string? AuthorId { get; set; }
    
    public string? UpdatedBy { get; set; } // Keep for backward compatibility, will map to ModifiedBy
    
    public virtual BlogCategory Category { get; set; } = null!;
    
    public virtual ApplicationUser? Author { get; set; }
}