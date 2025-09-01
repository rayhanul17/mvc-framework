using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.Blog.Models.Entities
{
    public class Comment : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        
        [Required]
        public required string Body { get; set; }
        
        public bool IsApproved { get; set; } = true;
        
        // Attachment support
        public string? AttachmentPath { get; set; }
        public string? AttachmentFileName { get; set; }
        public long? AttachmentSize { get; set; }
        public string? AttachmentContentType { get; set; }
        
        public virtual BlogPost Post { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}