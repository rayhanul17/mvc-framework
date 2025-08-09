using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class Ticket : BaseEntity
{
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string TicketNumber { get; set; } = string.Empty;
    
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    public TicketStatus Status { get; set; } = TicketStatus.New;
    
    [StringLength(100)]
    public string Category { get; set; } = "General";
    
    // Customer who created the ticket
    public string CustomerId { get; set; } = string.Empty;
    public virtual ApplicationUser Customer { get; set; } = null!;
    
    // Support staff assigned to the ticket
    public string? AssignedToId { get; set; }
    public virtual ApplicationUser? AssignedTo { get; set; }
    
    // Manager who assigned the ticket
    public string? AssignedById { get; set; }
    public virtual ApplicationUser? AssignedBy { get; set; }
    
    public DateTime? AssignedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ReopenedAt { get; set; }
    
    [StringLength(500)]
    public string? ResolutionNotes { get; set; }
    
    public int ResponseTimeHours { get; set; }
    public int ResolutionTimeHours { get; set; }
    
    // Navigation properties
    public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public virtual ICollection<TicketNotification> Notifications { get; set; } = new List<TicketNotification>();
    public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}

public enum TicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum TicketStatus
{
    New = 1,
    Assigned = 2,
    InProgress = 3,
    WaitingForCustomer = 4,
    UnderReview = 5,
    Resolved = 6,
    Closed = 7,
    Reopened = 8,
    Cancelled = 9
}