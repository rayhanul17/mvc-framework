using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class TicketNotification : BaseEntity
{
    
    public int TicketId { get; set; }
    public virtual Ticket Ticket { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    public bool IsEmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
}

public enum NotificationType
{
    NewTicket = 1,
    TicketAssigned = 2,
    StatusChanged = 3,
    NewComment = 4,
    TicketResolved = 5,
    TicketClosed = 6,
    TicketReopened = 7,
    PriorityChanged = 8,
    TicketEscalated = 9
}