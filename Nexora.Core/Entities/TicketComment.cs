using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketComment : BaseEntity
{
    
    public int TicketId { get; set; }
    public virtual Ticket Ticket { get; set; } = null!;
    
    [Required]
    public string Comment { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    public bool IsInternal { get; set; } // Internal notes not visible to customer
    
    public CommentType Type { get; set; } = CommentType.Comment;
    
    // For status change comments
    public TicketStatus? OldStatus { get; set; }
    public TicketStatus? NewStatus { get; set; }
    
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

public enum CommentType
{
    Comment = 1,
    StatusChange = 2,
    Assignment = 3,
    Resolution = 4,
    Reopened = 5,
    InternalNote = 6
}