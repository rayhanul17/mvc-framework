using Microsoft.AspNetCore.Http;
using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface ICommentService
{
    Task<Result<IEnumerable<Comment>>> GetCommentsByPostIdAsync(int blogPostId);
    Task<Result<Comment>> GetCommentByIdAsync(int commentId);
    Task<Result<Comment>> AddCommentAsync(Comment comment, IList<IFormFile>? attachments = null);
    Task<Result<Comment>> UpdateCommentAsync(Comment comment);
    Task<Result<bool>> DeleteCommentAsync(int commentId);
    Task<Result<bool>> ApproveCommentAsync(int commentId);
    Task<Result<bool>> RejectCommentAsync(int commentId, string? moderatorNotes = null);
    Task<Result<IEnumerable<Comment>>> GetPendingCommentsAsync();
    Task<Result<IEnumerable<Comment>>> GetCommentRepliesAsync(int parentCommentId);
}