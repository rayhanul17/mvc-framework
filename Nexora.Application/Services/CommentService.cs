using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;

namespace Nexora.Application.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CommentService> _logger;
    private readonly IWebHostEnvironment _environment;

    public CommentService(
        ApplicationDbContext context,
        ILogger<CommentService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    public async Task<Result<IEnumerable<Comment>>> GetCommentsByPostIdAsync(int blogPostId)
    {
        try
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.BlogPostId == blogPostId && c.ParentCommentId == null && !c.IsDeleted && c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Result<IEnumerable<Comment>>.Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {BlogPostId}", blogPostId);
            return Result<IEnumerable<Comment>>.Failure("Error retrieving comments");
        }
    }

    public async Task<Result<Comment>> GetCommentByIdAsync(int commentId)
    {
        try
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
                return Result<Comment>.Failure("Comment not found");

            return Result<Comment>.Success(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment {CommentId}", commentId);
            return Result<Comment>.Failure("Error retrieving comment");
        }
    }

    public async Task<Result<Comment>> AddCommentAsync(Comment comment, IList<IFormFile>? attachments = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            comment.CreatedAt = DateTime.UtcNow;
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Handle file attachments
            if (attachments != null && attachments.Any())
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "comments", comment.Id.ToString());
                Directory.CreateDirectory(uploadPath);

                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var attachment = new CommentAttachment
                        {
                            CommentId = comment.Id,
                            FileName = file.FileName,
                            FilePath = Path.Combine("uploads", "comments", comment.Id.ToString(), fileName),
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            UploadedAt = DateTime.UtcNow
                        };

                        _context.CommentAttachments.Add(attachment);
                    }
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload comment with attachments
            var savedComment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return Result<Comment>.Success(savedComment!);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding comment");
            return Result<Comment>.Failure("Error adding comment");
        }
    }

    public async Task<Result<Comment>> UpdateCommentAsync(Comment comment)
    {
        try
        {
            var existingComment = await _context.Comments.FindAsync(comment.Id);
            if (existingComment == null)
                return Result<Comment>.Failure("Comment not found");

            existingComment.Content = comment.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<Comment>.Success(existingComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", comment.Id);
            return Result<Comment>.Failure("Error updating comment");
        }
    }

    public async Task<Result<bool>> DeleteCommentAsync(int commentId)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return Result<bool>.Failure("Comment not found");

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return Result<bool>.Failure("Error deleting comment");
        }
    }

    public async Task<Result<bool>> ApproveCommentAsync(int commentId)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return Result<bool>.Failure("Comment not found");

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving comment {CommentId}", commentId);
            return Result<bool>.Failure("Error approving comment");
        }
    }

    public async Task<Result<bool>> RejectCommentAsync(int commentId, string? moderatorNotes = null)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return Result<bool>.Failure("Comment not found");

            comment.IsApproved = false;
            comment.ModeratorNotes = moderatorNotes;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting comment {CommentId}", commentId);
            return Result<bool>.Failure("Error rejecting comment");
        }
    }

    public async Task<Result<IEnumerable<Comment>>> GetPendingCommentsAsync()
    {
        try
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.BlogPost)
                .Include(c => c.Attachments)
                .Where(c => !c.IsApproved && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Result<IEnumerable<Comment>>.Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending comments");
            return Result<IEnumerable<Comment>>.Failure("Error retrieving pending comments");
        }
    }

    public async Task<Result<IEnumerable<Comment>>> GetCommentRepliesAsync(int parentCommentId)
    {
        try
        {
            var replies = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted && c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Result<IEnumerable<Comment>>.Success(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment replies for {ParentCommentId}", parentCommentId);
            return Result<IEnumerable<Comment>>.Failure("Error retrieving comment replies");
        }
    }

    public async Task<CommentAttachment?> GetAttachmentByIdAsync(int attachmentId)
    {
        try
        {
            return await _context.CommentAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachment {AttachmentId}", attachmentId);
            return null;
        }
    }
}