using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Core.Entities;

public class TicketAttachment : BaseEntity
{
    
    public int? TicketId { get; set; }
    public virtual Ticket? Ticket { get; set; }
    
    public int? CommentId { get; set; }
    public virtual TicketComment? Comment { get; set; }
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public string UploadedById { get; set; } = string.Empty;
    public virtual ApplicationUser UploadedBy { get; set; } = null!;
}