using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class RoleUrlPermission : BaseEntity
{
    [Required]
    public string RoleId { get; set; } = string.Empty;
    
    [Required]
    public string Url { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public virtual ApplicationRole Role { get; set; } = null!;
}