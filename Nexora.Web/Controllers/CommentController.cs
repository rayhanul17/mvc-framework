using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using System.Security.Claims;

namespace Nexora.Web.Controllers;

[AllowAnonymous]
public class CommentController : Controller
{
    private readonly ICommentService _commentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CommentController> _logger;

    public CommentController(
        ICommentService commentService,
        UserManager<ApplicationUser> userManager,
        ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(int blogPostId)
    {
        var result = await _commentService.GetCommentsByPostIdAsync(blogPostId);
        
        if (result.IsSuccess)
        {
            // Map comments to include proper user information
            var mappedComments = result.Data.Select(c => new
            {
                id = c.Id,
                content = c.Content,
                authorName = !string.IsNullOrEmpty(c.AuthorName) ? c.AuthorName : "Anonymous User",
                authorEmail = c.AuthorEmail,
                userId = c.UserId, // Include this to differentiate between authenticated and anonymous users
                createdAt = c.CreatedAt,
                attachments = c.Attachments?.Select(a => new
                {
                    id = a.Id,
                    fileName = a.FileName,
                    filePath = a.FilePath
                })
            });
            
            return Json(new { success = true, data = mappedComments });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int blogPostId, string content, string? authorName = null, string? authorEmail = null, IList<IFormFile>? attachments = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment content is required" });
            }

            var comment = new Comment
            {
                BlogPostId = blogPostId,
                Content = content.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            // Check if user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                
                if (user != null)
                {
                    comment.UserId = userId;
                    comment.AuthorName = user.FullName;
                    comment.AuthorEmail = user.Email;
                    comment.IsApproved = true; // Auto-approve authenticated users
                }
            }
            else
            {
                // Anonymous comment
                if (string.IsNullOrWhiteSpace(authorName))
                {
                    return Json(new { success = false, message = "Author name is required for anonymous comments" });
                }

                comment.AuthorName = authorName.Trim();
                comment.AuthorEmail = authorEmail?.Trim();
                comment.IsApproved = true; // You can change this to require moderation
            }

            var result = await _commentService.AddCommentAsync(comment, attachments);

            if (result.IsSuccess)
            {
                return Json(new { 
                    success = true, 
                    message = "Comment added successfully",
                    data = new {
                        id = result.Data!.Id,
                        content = result.Data.Content,
                        authorName = result.Data.AuthorName,
                        createdAt = result.Data.CreatedAt,
                        attachments = result.Data.Attachments?.Select(a => new {
                            id = a.Id,
                            fileName = a.FileName,
                            filePath = a.FilePath
                        })
                    }
                });
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return Json(new { success = false, message = "An error occurred while adding the comment" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddReply(int parentCommentId, string content, string? authorName = null, string? authorEmail = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Reply content is required" });
            }

            // Get the parent comment to get the blog post ID
            var parentResult = await _commentService.GetCommentByIdAsync(parentCommentId);
            if (!parentResult.IsSuccess)
            {
                return Json(new { success = false, message = "Parent comment not found" });
            }

            var reply = new Comment
            {
                BlogPostId = parentResult.Data!.BlogPostId,
                ParentCommentId = parentCommentId,
                Content = content.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            // Check if user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                
                if (user != null)
                {
                    reply.UserId = userId;
                    reply.AuthorName = user.FullName;
                    reply.AuthorEmail = user.Email;
                    reply.IsApproved = true;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(authorName))
                {
                    return Json(new { success = false, message = "Author name is required for anonymous replies" });
                }

                reply.AuthorName = authorName.Trim();
                reply.AuthorEmail = authorEmail?.Trim();
                reply.IsApproved = true;
            }

            var result = await _commentService.AddCommentAsync(reply);

            if (result.IsSuccess)
            {
                return Json(new { 
                    success = true, 
                    message = "Reply added successfully",
                    data = new {
                        id = result.Data!.Id,
                        content = result.Data.Content,
                        authorName = result.Data.AuthorName,
                        createdAt = result.Data.CreatedAt,
                        parentCommentId = result.Data.ParentCommentId
                    }
                });
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reply");
            return Json(new { success = false, message = "An error occurred while adding the reply" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        try
        {
            var commentResult = await _commentService.GetCommentByIdAsync(commentId);
            if (!commentResult.IsSuccess)
            {
                return Json(new { success = false, message = "Comment not found" });
            }

            var comment = commentResult.Data!;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Only allow the author or admins to delete
            if (comment.UserId != userId && !User.IsInRole("Administrator") && !User.IsInRole("SuperAdmin"))
            {
                return Json(new { success = false, message = "You are not authorized to delete this comment" });
            }

            var result = await _commentService.DeleteCommentAsync(commentId);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Comment deleted successfully" });
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return Json(new { success = false, message = "An error occurred while deleting the comment" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            // This would need to be implemented with proper security checks
            // For now, return a placeholder
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return NotFound();
        }
    }

    // Admin endpoints
    [HttpGet]
    [Authorize(Roles = "Administrator,SuperAdmin")]
    public async Task<IActionResult> GetPendingComments()
    {
        var result = await _commentService.GetPendingCommentsAsync();
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, data = result.Data });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,SuperAdmin")]
    public async Task<IActionResult> ApproveComment(int commentId)
    {
        var result = await _commentService.ApproveCommentAsync(commentId);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "Comment approved successfully" });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,SuperAdmin")]
    public async Task<IActionResult> RejectComment(int commentId, string? moderatorNotes = null)
    {
        var result = await _commentService.RejectCommentAsync(commentId, moderatorNotes);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "Comment rejected successfully" });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }
}