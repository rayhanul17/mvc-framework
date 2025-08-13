using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketComment : BaseEntity
{
    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;  // Changed from Content to Comment
    
    public CommentType Type { get; set; } = CommentType.General;
    
    public TicketStatus? OldStatus { get; set; }
    public TicketStatus? NewStatus { get; set; }
    
    public bool IsInternal { get; set; } = false;
    
    public bool IsSystemGenerated { get; set; } = false;
    
    // Foreign Keys
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    
    // Collections
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

public enum CommentType
{
    General = 1,
    StatusChange = 2,
    Assignment = 3,
    Internal = 4,
    Resolution = 5,
    Reopened = 6,
    Comment = 7
}
