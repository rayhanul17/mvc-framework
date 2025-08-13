using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class Ticket : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public TicketStatus Status { get; set; } = TicketStatus.New;
    
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    [StringLength(50)]
    public string? TicketNumber { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(100)]
    public string? SubCategory { get; set; }
    
    [StringLength(500)]
    public string? ResolutionNotes { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public DateTime? AssignedAt { get; set; }
    
    public DateTime? ReopenedAt { get; set; }
    
    public double? ResponseTimeHours { get; set; }
    
    public double? ResolutionTimeHours { get; set; }
    
    // Foreign Keys
    public string CustomerId { get; set; } = string.Empty;  // CreatedByUser
    public string? AssignedToId { get; set; }
    public string? AssignedById { get; set; }
    public string? LastModifiedByUserId { get; set; }
    
    // Navigation Properties
    public virtual ApplicationUser Customer { get; set; } = null!;  // The user who created the ticket
    public virtual ApplicationUser? AssignedTo { get; set; }
    public virtual ApplicationUser? AssignedBy { get; set; }
    public virtual ApplicationUser? LastModifiedBy { get; set; }
    
    // Collections
    public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public virtual ICollection<TicketNotification> Notifications { get; set; } = new List<TicketNotification>();
}

public enum TicketStatus
{
    New = 1,
    Open = 2,
    InProgress = 3,
    OnHold = 4,
    Resolved = 5,
    Closed = 6,
    Cancelled = 7,
    Reopened = 8,
    Assigned = 9,
    WaitingForCustomer = 10,
    UnderReview = 11
}

public enum TicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4,
    Critical = 5
}
