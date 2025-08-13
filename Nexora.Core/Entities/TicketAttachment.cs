using System.ComponentModel.DataAnnotations;

namespace Nexora.Core.Entities;

public class TicketAttachment : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [StringLength(50)]
    public string? FileType { get; set; }  // File extension/type
    
    [StringLength(100)]
    public string? ContentType { get; set; }
    
    // Foreign Keys
    public int? TicketId { get; set; }
    public int? CommentId { get; set; }
    public string UploadedById { get; set; } = string.Empty;  // Changed from UploadedByUserId
    
    // Navigation Properties
    public virtual Ticket? Ticket { get; set; }
    public virtual TicketComment? Comment { get; set; }
    public virtual ApplicationUser UploadedBy { get; set; } = null!;
}
