using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Core.Entities;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }

    // For authenticated users
    public string? UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }

    // Blog post relationship
    [Required]
    public int BlogPostId { get; set; }
    [ForeignKey("BlogPostId")]
    public virtual BlogPost BlogPost { get; set; } = null!;

    // Parent comment for replies
    public int? ParentCommentId { get; set; }
    [ForeignKey("ParentCommentId")]
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    // File attachments
    public virtual ICollection<CommentAttachment> Attachments { get; set; } = new List<CommentAttachment>();

    // Status and moderation
    public bool IsApproved { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string? ModeratorNotes { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}