using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModularHost.Web.Core.Models.Entities;

namespace ModularHost.Web.Modules.Blog.Models.Entities
{
    public class Comment : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        
        [Required]
        public required string Body { get; set; }
        
        public bool IsApproved { get; set; } = true;
        public bool IsDeleted { get; set; }
        
        public virtual BlogPost Post { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}