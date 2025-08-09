using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class BlogCategory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Slug { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? UpdatedBy { get; set; } // Keep for backward compatibility, will map to ModifiedBy
    
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}