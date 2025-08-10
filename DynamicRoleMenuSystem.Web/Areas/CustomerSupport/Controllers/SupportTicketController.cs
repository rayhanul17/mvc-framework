using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Models;
using DynamicRoleMenuSystem.Web.Controllers;

namespace DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class SupportTicketController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<SupportTicketController> _logger;

    public SupportTicketController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService,
        ILogger<SupportTicketController> logger)
    {
        _ticketService = ticketService;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<IActionResult> MyTickets(string filterStatus = "all")
    {
        var userId = GetCurrentUserId();
        var ticketsResult = await _ticketService.GetAssignedTicketsAsync(userId);
        
        if (!ticketsResult.IsSuccess)
        {
            SetErrorMessage(ticketsResult.ErrorMessage);
            return View(new List<Ticket>());
        }

        var tickets = ticketsResult.Data;

        // Apply filter
        if (filterStatus != "all" && Enum.TryParse<TicketStatus>(filterStatus, true, out var status))
        {
            tickets = tickets.Where(t => t.Status == status).ToList();
        }

        ViewBag.FilterStatus = filterStatus;
        return View(tickets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            SetErrorMessage(ticketResult.ErrorMessage);
            return RedirectToAction(nameof(MyTickets));
        }

        // Check if user is assigned to this ticket
        var userId = GetCurrentUserId();
        // Check if user has permission to view this ticket
        // User can view if they are assigned to it or have manage permissions
        if (ticketResult.Data.AssignedToId != userId)
        {
            // Additional permission check could be added here based on menu permissions
            // For now, allow if user has access to this action
        }

        var commentsResult = await _ticketService.GetTicketCommentsAsync(id, true);
        var attachmentsResult = await _ticketService.GetTicketAttachmentsAsync(id);
        var historyResult = await _ticketService.GetTicketHistoryAsync(id);

        var model = new TicketDetailsViewModel
        {
            Ticket = ticketResult.Data,
            Comments = commentsResult.IsSuccess ? commentsResult.Data : new List<TicketComment>(),
            Attachments = attachmentsResult.IsSuccess ? attachmentsResult.Data : new List<TicketAttachment>(),
            History = historyResult.IsSuccess ? historyResult.Data : new List<TicketHistory>(),
            CanEdit = true,
            CanResolve = ticketResult.Data.Status == TicketStatus.InProgress,
            CanClose = false,
            CanReopen = false
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartProgress(int ticketId)
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.UpdateTicketStatusAsync(
            ticketId, 
            TicketStatus.InProgress, 
            userId, 
            "Started working on ticket"
        );

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket status updated to In Progress.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpGet]
    public async Task<IActionResult> Resolve(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            SetErrorMessage(ticketResult.ErrorMessage);
            return RedirectToAction(nameof(MyTickets));
        }

        var model = new ResolveTicketViewModel
        {
            TicketId = id,
            TicketNumber = ticketResult.Data.TicketNumber
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(ResolveTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var result = await _ticketService.ResolveTicketAsync(
            model.TicketId, 
            model.ResolutionNotes, 
            userId
        );

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket resolved successfully.");
            return RedirectToAction(nameof(MyTickets));
        }

        SetErrorMessage(result.ErrorMessage);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        var userId = GetCurrentUserId();
        var comment = new TicketComment
        {
            TicketId = model.TicketId,
            UserId = userId,
            Comment = model.Comment,
            IsInternal = model.IsInternal,
            Type = CommentType.Comment
        };

        var result = await _ticketService.AddCommentAsync(comment);

        if (result.IsSuccess)
        {
            // Handle attachments if any
            if (model.Attachments != null && model.Attachments.Any())
            {
                foreach (var file in model.Attachments)
                {
                    var uploadPath = await _fileUploadService.UploadImageAsync(file);
                    if (!string.IsNullOrEmpty(uploadPath))
                    {
                        var attachment = new TicketAttachment
                        {
                            TicketId = model.TicketId,
                            CommentId = result.Data.Id,
                            FileName = file.FileName,
                            FilePath = uploadPath,
                            FileSize = file.Length,
                            FileType = file.ContentType,
                            UploadedById = userId
                        };
                        await _ticketService.AddAttachmentAsync(attachment);
                    }
                }
            }

            SetSuccessMessage("Comment added successfully.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReview(int ticketId, string notes)
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.UpdateTicketStatusAsync(
            ticketId, 
            TicketStatus.Resolved, 
            userId, 
            notes ?? "Requesting review for resolution"
        );

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket sent for review.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(MyTickets));
    }
}