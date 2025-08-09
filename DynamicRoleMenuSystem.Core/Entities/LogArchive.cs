using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class LogArchive : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    public int EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    
    public string? OldValues { get; set; } // JSON string of old values
    
    public string? NewValues { get; set; } // JSON string of new values
    
    [StringLength(500)]
    public string? Changes { get; set; } // Summary of changes
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime LoggedAt { get; set; }
    
    public DateTime ArchivedAt { get; set; }
    
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
}