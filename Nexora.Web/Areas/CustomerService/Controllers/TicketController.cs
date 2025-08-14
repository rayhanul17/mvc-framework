using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerService.Models;
using Nexora.Web.Controllers;
using System.Security.Claims;

namespace Nexora.Web.Areas.CustomerService.Controllers;

[Area("CustomerService")]
[Authorize]
public class TicketController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService,
        ILogger<TicketController> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(TicketFilterModel filter, int page = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = await GetUserCustomerServiceRoles();
        
        var query = _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        // Apply role-based filtering
        if (userRoles.Contains("CustomerSupportCustomer") && 
            !userRoles.Contains("CustomerSupportAgent") && 
            !userRoles.Contains("CustomerSupportManager") && 
            !userRoles.Contains("CustomerSupportAdmin"))
        {
            query = query.Where(t => t.CustomerId == userId);
        }
        else if (userRoles.Contains("CustomerSupportAgent") && 
                 !userRoles.Contains("CustomerSupportManager") && 
                 !userRoles.Contains("CustomerSupportAdmin"))
        {
            query = query.Where(t => t.AssignedToId == userId || t.CustomerId == userId);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(t => 
                t.Title.Contains(filter.SearchTerm) || 
                t.Description.Contains(filter.SearchTerm) ||
                t.TicketNumber.Contains(filter.SearchTerm));
        }

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(t => t.Category == filter.Category);

        if (!string.IsNullOrEmpty(filter.AssignedToId))
            query = query.Where(t => t.AssignedToId == filter.AssignedToId);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo);

        // Apply sorting
        query = filter.SortBy switch
        {
            "Title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "Status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "Priority" => filter.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "DueDate" => filter.SortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            _ => filter.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        var pageSize = 20;
        var totalCount = await query.CountAsync();
        var tickets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketItemViewModel
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber ?? $"T{t.Id:D6}",
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Category = t.Category,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DueDate = t.DueDate,
                CreatedByName = t.Customer.FullName,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                CommentCount = t.Comments.Count(),
                AttachmentCount = t.Attachments.Count()
            })
            .ToListAsync();

        var viewModel = new TicketListViewModel
        {
            Tickets = tickets,
            Filter = filter,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateTicketViewModel();
        await PopulateDropdowns(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model);
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ticketNumber = await GenerateTicketNumber();

        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            Priority = model.Priority,
            Status = TicketStatus.New,
            Category = model.Category,
            SubCategory = model.SubCategory,
            DueDate = model.DueDate,
            TicketNumber = ticketNumber,
            CustomerId = userId,
            AssignedToId = model.AssignedToUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Handle file attachments
        if (model.Attachments != null && model.Attachments.Any())
        {
            foreach (var file in model.Attachments)
            {
                var uploadResult = await _fileUploadService.UploadImageAsync(file, "tickets");
                if (!string.IsNullOrEmpty(uploadResult))
                {
                    var attachment = new TicketAttachment
                    {
                        TicketId = ticket.Id,
                        FileName = file.FileName,
                        FilePath = uploadResult,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedById = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TicketAttachments.Add(attachment);
                }
            }
            await _context.SaveChangesAsync();
        }

        // Add creation history
        await AddTicketHistory(ticket.Id, "Created", null, ticket.Status.ToString(), $"Ticket created");

        // Send notification if assigned
        if (!string.IsNullOrEmpty(ticket.AssignedToId))
        {
            await CreateNotification(ticket.Id, ticket.AssignedToId, 
                "New Ticket Assigned", 
                $"Ticket #{ticket.TicketNumber} has been assigned to you", 
                NotificationType.Assignment);
        }

        SetSuccessMessage($"Ticket #{ticket.TicketNumber} has been created successfully.");
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Attachments)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.UploadedBy)
            .Include(t => t.History)
                .ThenInclude(h => h.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = await GetUserCustomerServiceRoles();

        // Check access
        if (!await CanAccessTicket(ticket, userId, userRoles))
            return Forbid();

        var viewModel = new TicketDetailsViewModel
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber ?? $"T{ticket.Id:D6}",
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Category = ticket.Category,
            SubCategory = ticket.SubCategory,
            DueDate = ticket.DueDate,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,
            CreatedByUserId = ticket.CustomerId,
            CreatedByName = ticket.Customer.FullName,
            CreatedByEmail = ticket.Customer.Email,
            AssignedToUserId = ticket.AssignedToId,
            AssignedToName = ticket.AssignedTo?.FullName,
            AssignedToEmail = ticket.AssignedTo?.Email,
            Comments = ticket.Comments.OrderBy(c => c.CreatedAt).Select(c => new TicketCommentViewModel
            {
                Id = c.Id,
                Content = c.Comment,
                IsInternal = c.IsInternal,
                IsSystemGenerated = c.IsSystemGenerated,
                CreatedAt = c.CreatedAt,
                UserName = c.User.FullName,
                UserAvatar = c.User.AvatarUrl,
                Attachments = c.Attachments.ToList()
            }).ToList(),
            Attachments = ticket.Attachments.ToList(),
            History = ticket.History.OrderByDescending(h => h.CreatedAt).Select(h => new TicketHistoryViewModel
            {
                Id = h.Id,
                Action = h.Action,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                Description = h.Description,
                CreatedAt = h.CreatedAt,
                UserName = h.User.FullName
            }).ToList(),
            CanEdit = await CanEditTicket(ticket, userId, userRoles),
            CanDelete = await CanDeleteTicket(ticket, userId, userRoles),
            CanAssign = await CanAssignTicket(userRoles),
            CanChangeStatus = await CanChangeStatus(ticket, userId, userRoles),
            CanComment = true
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid comment data" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var comment = new TicketComment
        {
            TicketId = model.TicketId,
            Comment = model.Content,
            IsInternal = model.IsInternal,
            IsSystemGenerated = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();

        // Handle attachments
        if (model.Attachments != null && model.Attachments.Any())
        {
            foreach (var file in model.Attachments)
            {
                var uploadResult = await _fileUploadService.UploadImageAsync(file, "comments");
                if (!string.IsNullOrEmpty(uploadResult))
                {
                    var attachment = new TicketAttachment
                    {
                        CommentId = comment.Id,
                        FileName = file.FileName,
                        FilePath = uploadResult,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedById = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TicketAttachments.Add(attachment);
                }
            }
            await _context.SaveChangesAsync();
        }

        // Update ticket
        var ticket = await _context.Tickets.FindAsync(model.TicketId);
        if (ticket != null)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        SetSuccessMessage("Comment added successfully.");
        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TicketStatus newStatus, string? statusNote)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var oldStatus = ticket.Status;
        
        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.LastModifiedByUserId = userId;

        if (newStatus == TicketStatus.Resolved)
            ticket.ResolvedAt = DateTime.UtcNow;
        else if (newStatus == TicketStatus.Closed)
            ticket.ClosedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Add history
        await AddTicketHistory(ticket.Id, "Status Changed", oldStatus.ToString(), newStatus.ToString(), statusNote);

        // Add system comment
        if (!string.IsNullOrEmpty(statusNote))
        {
            var comment = new TicketComment
            {
                TicketId = ticket.Id,
                Comment = $"Status changed from {oldStatus} to {newStatus}. {statusNote}",
                IsInternal = false,
                IsSystemGenerated = true,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        SetSuccessMessage($"Ticket status updated to {newStatus}.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // Helper methods
    private async Task<List<string>> GetUserCustomerServiceRoles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.GetUserAsync(User);
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var csRoleMappings = await _context.CustomerServiceRoleMappings
            .Where(m => m.IsActive && userRoles.Contains(m.AspNetRoleName))
            .Select(m => m.CustomerServiceRole)
            .ToListAsync();
        
        return csRoleMappings;
    }

    private async Task<bool> CanAccessTicket(Ticket ticket, string userId, List<string> userRoles)
    {
        if (userRoles.Contains("CustomerSupportAdmin") || userRoles.Contains("CustomerSupportManager"))
            return true;
        
        if (userRoles.Contains("CustomerSupportAgent") && ticket.AssignedToId == userId)
            return true;
        
        if (ticket.CustomerId == userId)
            return true;
        
        return false;
    }

    private async Task<bool> CanEditTicket(Ticket ticket, string userId, List<string> userRoles)
    {
        if (userRoles.Contains("CustomerSupportAdmin") || userRoles.Contains("CustomerSupportManager"))
            return true;
        
        if (userRoles.Contains("CustomerSupportAgent") && ticket.AssignedToId == userId)
            return true;
        
        if (ticket.CustomerId == userId && ticket.Status == TicketStatus.New)
            return true;
        
        return false;
    }

    private async Task<bool> CanDeleteTicket(Ticket ticket, string userId, List<string> userRoles)
    {
        return userRoles.Contains("CustomerSupportAdmin");
    }

    private async Task<bool> CanAssignTicket(List<string> userRoles)
    {
        return userRoles.Contains("CustomerSupportAdmin") || userRoles.Contains("CustomerSupportManager");
    }

    private async Task<bool> CanChangeStatus(Ticket ticket, string userId, List<string> userRoles)
    {
        if (userRoles.Contains("CustomerSupportAdmin") || userRoles.Contains("CustomerSupportManager"))
            return true;
        
        if (userRoles.Contains("CustomerSupportAgent") && ticket.AssignedToId == userId)
            return true;
        
        return false;
    }

    private async Task<string> GenerateTicketNumber()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _context.Tickets.CountAsync(t => t.CreatedAt.Date == DateTime.Today);
        return $"T{date}{(count + 1):D4}";
    }

    private async Task AddTicketHistory(int ticketId, string action, string? oldValue, string? newValue, string? description)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var history = new TicketHistory
        {
            TicketId = ticketId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.TicketHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    private async Task CreateNotification(int ticketId, string userId, string title, string message, NotificationType type)
    {
        var notification = new TicketNotification
        {
            TicketId = ticketId,
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.TicketNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    private async Task PopulateDropdowns(CreateTicketViewModel model)
    {
        // Get agents for assignment
        var agentRole = await _context.CustomerServiceRoleMappings
            .Where(m => m.CustomerServiceRole == "CustomerSupportAgent" && m.IsActive)
            .Select(m => m.AspNetRoleName)
            .FirstOrDefaultAsync();
        
        if (!string.IsNullOrEmpty(agentRole))
        {
            var agents = await _userManager.GetUsersInRoleAsync(agentRole);
            model.AvailableAgents = agents
                .Where(u => !u.IsSuperAdmin) // Exclude SuperAdmin users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                }).ToList();
        }

        // Add categories
        model.Categories = new List<SelectListItem>
        {
            new SelectListItem { Value = "Technical", Text = "Technical Issue" },
            new SelectListItem { Value = "Billing", Text = "Billing & Payment" },
            new SelectListItem { Value = "Account", Text = "Account Management" },
            new SelectListItem { Value = "Feature", Text = "Feature Request" },
            new SelectListItem { Value = "Other", Text = "Other" }
        };
    }
}