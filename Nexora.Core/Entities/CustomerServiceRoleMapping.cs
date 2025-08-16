using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class CustomerServiceRoleMapping : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string CustomerServiceRole { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string AspNetRoleName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ApplicationRole? AspNetRole { get; set; }
}
