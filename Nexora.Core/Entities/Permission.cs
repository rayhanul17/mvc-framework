using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "BlogPost.Create"
    
    [StringLength(50)]
    public string Area { get; set; } = string.Empty; // e.g., "CustomerSupport"
    
    [Required]
    [StringLength(50)]
    public string Controller { get; set; } = string.Empty; // e.g., "BlogPost"
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // e.g., "Create"
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}