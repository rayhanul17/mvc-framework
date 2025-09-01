using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MRCMS.Core.Models;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.Blog.Models.Entities
{
    public class BlogPost : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public required string Title { get; set; }
        
        [Required]
        [StringLength(255)]
        public required string Slug { get; set; }
        
        [StringLength(500)]
        public string Summary { get; set; } = string.Empty;
        
        [Required]
        public required string Content { get; set; }
        
        public string? FeaturedImage { get; set; }
        
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public bool AllowComments { get; set; } = true;
        public Guid AuthorId { get; set; }
        public Guid? CategoryId { get; set; }
        
        [StringLength(255)]
        public string? MetaTitle { get; set; }
        
        [StringLength(255)]
        public string? MetaDescription { get; set; }
        
        [StringLength(500)]
        public string? MetaKeywords { get; set; }
        
        public virtual User Author { get; set; } = null!;
        public virtual Category? Category { get; set; }
        public virtual ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        
        public string TagNames => BlogPostTags?.Any() == true 
            ? string.Join(", ", BlogPostTags.Select(t => t.Tag?.Name ?? "")) 
            : string.Empty;
    }
}