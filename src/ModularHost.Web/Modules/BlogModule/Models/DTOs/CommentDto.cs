using System;
using System.Collections.Generic;

namespace MRCMS.Modules.Blog.Models.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }

    public class CreateCommentDto
    {
        public Guid PostId { get; set; }
        public required string Body { get; set; }
        public Guid? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public required string Body { get; set; }
        public bool IsApproved { get; set; }
    }
}