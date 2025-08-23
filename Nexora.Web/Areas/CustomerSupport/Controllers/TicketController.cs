using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Web.Attributes;
using Nexora.Web.Controllers;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[DynamicPermissionAuthorize]
public class TicketController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<TicketController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IPermissionService _permissionService;

    public TicketController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService,
        ILogger<TicketController> logger,
        ApplicationDbContext context,
        IPermissionService permissionService)
    {
        _ticketService = ticketService;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _logger = logger;
        _context = context;
        _permissionService = permissionService;
    }

    // GET: CustomerSupport/Ticket
    public async Task<IActionResult> Index(string status = "all", string priority = "all", string assignedTo = "all", string search = "")
    {
        var userId = GetCurrentUserId();
        var currentUser = await _userManager.GetUserAsync(User);
        // Check if user has any permission in CustomerSupport area (agent or manager)
        var isAgent = await _permissionService.UserHasAnyPermissionInAreaAsync(User, "CustomerSupport");
        
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
        
        if (priority != "all" && Enum.TryParse<TicketPriority>(priority, out var ticketPriority))
        {
            tickets = tickets.Where(t => t.Priority == ticketPriority).ToList();
        }
        
        if (assignedTo != "all")
        {
            if (assignedTo == "unassigned")
            {
                tickets = tickets.Where(t => t.AssignedToId == null).ToList();
            }
            else
            {
                tickets = tickets.Where(t => t.AssignedToId == assignedTo).ToList();
            }
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
            FilterPriority = priority,
            SearchTerm = search,
            TotalCount = tickets.Count
        };
        
        // Add support agents list if user is an agent
        if (isAgent)
        {
            ViewBag.SupportAgents = await GetSupportAgentsAsync();
            ViewBag.IsAgent = true;
        }

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
    
    // POST: CustomerSupport/Ticket/Assign
    [HttpPost]
    [PermissionAuthorize(area: "CustomerSupport", controller: "ManageTicket", action: "Assign")]
    public async Task<IActionResult> Assign(int id, string assignToUserId, string notes = "")
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return Json(new { success = false, message = "Ticket not found" });
        }

        var previousAgentId = ticket.AssignedToId;
        ticket.AssignedToId = assignToUserId;
        ticket.AssignedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.InProgress;
        
        _context.Update(ticket);
        
        // Add assignment history
        var history = new TicketHistory
        {
            TicketId = id,
            Action = "Assigned",
            Description = string.IsNullOrEmpty(notes) ? $"Ticket assigned to {assignToUserId}" : $"Ticket assigned to {assignToUserId}. Notes: {notes}",
            UserId = GetCurrentUserId()!,
            CreatedAt = DateTime.UtcNow
        };
        _context.TicketHistories.Add(history);
        
        await _context.SaveChangesAsync();
        
        SetSuccessMessage("Ticket assigned successfully");
        return Json(new { success = true, message = "Ticket assigned successfully" });
    }
    
    // POST: CustomerSupport/Ticket/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TicketStatus newStatus, string notes = "")
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return Json(new { success = false, message = "Ticket not found" });
        }

        var oldStatus = ticket.Status;
        ticket.Status = newStatus;
        
        if (newStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.ResolutionTimeHours = (int)(DateTime.UtcNow - ticket.CreatedAt).TotalHours;
        }
        else if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }

        _context.Update(ticket);
        
        // Add status change history
        var history = new TicketHistory
        {
            TicketId = id,
            Action = "Status Changed",
            Description = string.IsNullOrEmpty(notes) ? $"Status changed from {oldStatus} to {newStatus}" : $"Status changed from {oldStatus} to {newStatus}. Notes: {notes}",
            UserId = GetCurrentUserId()!,
            CreatedAt = DateTime.UtcNow
        };
        _context.TicketHistories.Add(history);
        
        await _context.SaveChangesAsync();
        
        SetSuccessMessage("Status updated successfully");
        return Json(new { success = true, message = "Status updated successfully" });
    }
    
    // GET: CustomerSupport/Ticket/Dashboard
    [PermissionAuthorize(area: "CustomerSupport", controller: "Ticket", action: "Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var model = new DashboardViewModel
        {
            Statistics = new TicketStatistics
            {
                TotalTickets = await _context.Tickets.CountAsync(),
                OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.New),
                InProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress),
                ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved),
                ClosedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed)
            },
            
            RecentTickets = await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync(),
                
            UnassignedTickets = await _context.Tickets
                .Include(t => t.Customer)
                .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync(),
                
            TicketsCreatedToday = await _context.Tickets
                .CountAsync(t => t.CreatedAt.Date == DateTime.Today),
                
            TicketsResolvedToday = await _context.Tickets
                .CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == DateTime.Today),
                
            OverdueTickets = await _context.Tickets
                .CountAsync(t => t.DueDate < DateTime.UtcNow && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved),
                
            HighPriorityTickets = await _context.Tickets
                .CountAsync(t => (t.Priority == TicketPriority.Critical || t.Priority == TicketPriority.Urgent) && 
                               t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
        };
        
        // Calculate average times
        model.AverageResolutionTime = await _context.Tickets
            .Where(t => t.ResolutionTimeHours.HasValue)
            .Select(t => t.ResolutionTimeHours!.Value)
            .DefaultIfEmpty(0)
            .AverageAsync();
            
        model.FirstResponseTime = await _context.Tickets
            .Where(t => t.ResponseTimeHours.HasValue)
            .Select(t => t.ResponseTimeHours!.Value)
            .DefaultIfEmpty(0)
            .AverageAsync();
        
        // Get agent performance
        model.TopPerformers = await GetAgentPerformanceAsync();
        
        return View(model);
    }
    
    // GET: CustomerSupport/Ticket/Track/{ticketNumber}
    public async Task<IActionResult> Track(string ticketNumber)
    {
        if (string.IsNullOrEmpty(ticketNumber))
        {
            SetErrorMessage("Please provide a ticket number");
            return View();
        }
        
        var ticket = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
            
        if (ticket == null)
        {
            SetErrorMessage("Ticket not found");
            return View();
        }
        
        // Check access - customers can only track their own tickets
        var userId = GetCurrentUserId();
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user!);
        var isAgent = roles.Any(r => r.Contains("Support") || r.Contains("Manager"));
        
        if (!isAgent && ticket.CustomerId != userId)
        {
            SetErrorMessage("You don't have permission to view this ticket");
            return View();
        }
        
        var history = await _context.TicketHistories
            .Where(h => h.TicketId == ticket.Id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
        
        ViewBag.Ticket = ticket;
        ViewBag.History = history;
        ViewBag.IsAgent = isAgent;
        
        return View(ticket);
    }
    
    // Helper methods
    private async Task<List<ApplicationUser>> GetSupportAgentsAsync()
    {
        var supportRoles = new[] { "CustomerSupportAgent", "CustomerSupportManager", "CustomerSupportAdmin" };
        var agents = new List<ApplicationUser>();

        foreach (var roleName in supportRoles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            agents.AddRange(usersInRole);
        }

        return agents.Distinct().ToList();
    }
    
    private async Task<List<AgentPerformance>> GetAgentPerformanceAsync()
    {
        var agents = await GetSupportAgentsAsync();
        var performance = new List<AgentPerformance>();

        foreach (var agent in agents)
        {
            var tickets = await _context.Tickets
                .Where(t => t.AssignedToId == agent.Id)
                .ToListAsync();

            performance.Add(new AgentPerformance
            {
                AgentId = agent.Id,
                AgentName = agent.FullName ?? agent.UserName ?? "Unknown",
                Email = agent.Email ?? "",
                TicketsAssigned = tickets.Count,
                TicketsResolved = tickets.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed),
                OpenTickets = tickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed),
                AverageResolutionTime = tickets.Where(t => t.ResolutionTimeHours.HasValue)
                    .Select(t => t.ResolutionTimeHours!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                LastActivity = tickets.Any() ? tickets.Max(t => t.UpdatedAt ?? t.CreatedAt) : DateTime.MinValue
            });
        }

        return performance.OrderByDescending(p => p.TicketsResolved).ToList();
    }
}