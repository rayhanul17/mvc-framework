using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketNotification : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    // Foreign Keys
    public int? TicketId { get; set; }  // Made nullable for general notifications
    public string UserId { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Ticket? Ticket { get; set; }  // Made nullable
    public virtual ApplicationUser User { get; set; } = null!;
}

public enum NotificationType
{
    Info = 1,
    Assignment = 2,
    StatusChange = 3,
    Comment = 4,
    Mention = 5,
    DueDate = 6,
    Escalation = 7,
    NewTicket = 8,
    TicketAssigned = 9,
    StatusChanged = 10,
    TicketResolved = 11,
    TicketClosed = 12,
    TicketReopened = 13
}
