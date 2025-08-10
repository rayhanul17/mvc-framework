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
public class TicketController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService,
        ILogger<TicketController> logger)
    {
        _ticketService = ticketService;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    // GET: CustomerSupport/Ticket
    public async Task<IActionResult> Index(string status = "all", string search = "")
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.GetTicketsAsync(userId);
        
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage);
            return View(new TicketListViewModel());
        }

        var tickets = result.Data;
        
        // Apply filters
        if (status != "all" && Enum.TryParse<TicketStatus>(status, out var ticketStatus))
        {
            tickets = tickets.Where(t => t.Status == ticketStatus).ToList();
        }
        
        if (!string.IsNullOrEmpty(search))
        {
            tickets = tickets.Where(t => 
                t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.TicketNumber.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var model = new TicketListViewModel
        {
            Tickets = tickets,
            FilterStatus = status,
            SearchTerm = search,
            TotalCount = tickets.Count
        };

        return View(model);
    }

    // GET: CustomerSupport/Ticket/Create
    public IActionResult Create()
    {
        return View(new CreateTicketViewModel());
    }

    // POST: CustomerSupport/Ticket/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            Priority = model.Priority,
            CustomerId = userId!,
            Status = TicketStatus.New
        };

        var result = await _ticketService.CreateTicketAsync(ticket);
        
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            return View(model);
        }

        // Handle file attachments
        if (model.Attachments != null && model.Attachments.Any())
        {
            foreach (var file in model.Attachments)
            {
                if (file.Length > 0)
                {
                    var uploadPath = await _fileUploadService.UploadImageAsync(file, "uploads/tickets");
                    if (!string.IsNullOrEmpty(uploadPath))
                    {
                        var attachment = new TicketAttachment
                        {
                            TicketId = result.Data.Id,
                            FileName = file.FileName,
                            FilePath = uploadPath,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            UploadedById = userId!
                        };
                        await _ticketService.AddAttachmentAsync(attachment);
                    }
                }
            }
        }

        SetSuccessMessage($"Ticket #{result.Data.TicketNumber} created successfully!");
        return RedirectToAction(nameof(Details), new { id = result.Data.Id });
    }

    // GET: CustomerSupport/Ticket/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            SetErrorMessage("Ticket not found");
            return RedirectToAction(nameof(Index));
        }

        var ticket = ticketResult.Data;
        var userId = GetCurrentUserId();
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user!);
        
        // Check access - customers can only see their own tickets
        if (!roles.Contains("Manager") && !roles.Contains("Support") && ticket.CustomerId != userId)
        {
            return Forbid();
        }

        var commentsResult = await _ticketService.GetTicketCommentsAsync(id, roles.Contains("Manager") || roles.Contains("Support"));
        var attachmentsResult = await _ticketService.GetTicketAttachmentsAsync(id);
        var historyResult = await _ticketService.GetTicketHistoryAsync(id);

        var model = new TicketDetailsViewModel
        {
            Ticket = ticket,
            Comments = commentsResult.IsSuccess ? commentsResult.Data : new List<TicketComment>(),
            Attachments = attachmentsResult.IsSuccess ? attachmentsResult.Data : new List<TicketAttachment>(),
            History = historyResult.IsSuccess ? historyResult.Data : new List<TicketHistory>(),
            CanEdit = ticket.CustomerId == userId && ticket.Status == TicketStatus.New,
            CanReopen = ticket.CustomerId == userId && (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Resolved)
        };

        return View(model);
    }

    // POST: CustomerSupport/Ticket/AddComment
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
            Comment = model.Comment,
            UserId = userId!,
            IsInternal = false,
            Type = CommentType.Comment
        };

        var result = await _ticketService.AddCommentAsync(comment);
        
        if (result.IsSuccess)
        {
            // Handle attachments
            if (model.Attachments != null && model.Attachments.Any())
            {
                foreach (var file in model.Attachments)
                {
                    if (file.Length > 0)
                    {
                        var uploadPath = await _fileUploadService.UploadImageAsync(file, "uploads/tickets");
                        if (!string.IsNullOrEmpty(uploadPath))
                        {
                            var attachment = new TicketAttachment
                            {
                                CommentId = result.Data.Id,
                                FileName = file.FileName,
                                FilePath = uploadPath,
                                FileType = file.ContentType,
                                FileSize = file.Length,
                                UploadedById = userId!
                            };
                            await _ticketService.AddAttachmentAsync(attachment);
                        }
                    }
                }
            }
            
            SetSuccessMessage("Comment added successfully!");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    // POST: CustomerSupport/Ticket/Reopen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(int id, string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            SetErrorMessage("Please provide a reason for reopening the ticket");
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = GetCurrentUserId();
        var result = await _ticketService.ReopenTicketAsync(id, reason, userId!);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket reopened successfully!");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: CustomerSupport/Ticket/Notifications
    public async Task<IActionResult> Notifications()
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.GetUserNotificationsAsync(userId!);
        
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage);
            return View(new List<TicketNotification>());
        }

        return View(result.Data);
    }

    // POST: CustomerSupport/Ticket/MarkNotificationRead
    [HttpPost]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        await _ticketService.MarkNotificationAsReadAsync(id);
        return Ok();
    }

    // GET: CustomerSupport/Ticket/GetUnreadCount
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.GetUnreadNotificationCountAsync(userId!);
        return Json(new { count = result.IsSuccess ? result.Data : 0 });
    }
}