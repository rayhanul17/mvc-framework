using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Core.Services.Interfaces;

namespace ModularHost.Web.Modules.Blog.Services.Interfaces
{
    public interface ICommentService : IBaseService<Comment, CommentDto, CreateCommentDto, UpdateCommentDto>
    {
        Task<IEnumerable<CommentDto>> GetPostCommentsAsync(Guid postId, bool includeReplies = true);
        Task<IEnumerable<CommentDto>> GetUserCommentsAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<CommentDto>> GetPendingCommentsAsync(int page, int pageSize);
        Task<bool> ApproveCommentAsync(Guid commentId);
        Task<bool> RejectCommentAsync(Guid commentId);
        Task<bool> SoftDeleteCommentAsync(Guid commentId);
    }
}