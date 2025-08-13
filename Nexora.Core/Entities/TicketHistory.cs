using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketHistory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? OldValue { get; set; }
    
    [StringLength(500)]
    public string? NewValue { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    // Foreign Keys
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}
