using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketHistory : BaseEntity
{
    
    public int TicketId { get; set; }
    public virtual Ticket Ticket { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? OldValue { get; set; }
    
    [StringLength(100)]
    public string? NewValue { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}