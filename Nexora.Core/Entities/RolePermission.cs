using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class RolePermission : BaseEntity
{
    [Required]
    [StringLength(450)]
    public string RoleId { get; set; } = string.Empty;
    
    [Required]
    public int PermissionId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ApplicationRole Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}