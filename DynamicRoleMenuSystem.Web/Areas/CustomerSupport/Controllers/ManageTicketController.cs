using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Models;
using DynamicRoleMenuSystem.Web.Controllers;

namespace DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class ManageTicketController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ManageTicketController> _logger;

    public ManageTicketController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        ILogger<ManageTicketController> logger)
    {
        _ticketService = ticketService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string filterStatus = "all", string searchTerm = "")
    {
        var ticketsResult = await _ticketService.GetTicketsAsync();
        if (!ticketsResult.IsSuccess)
        {
            SetErrorMessage(ticketsResult.ErrorMessage);
            return View(new TicketListViewModel());
        }

        var tickets = ticketsResult.Data;

        // Apply filters
        if (filterStatus != "all" && Enum.TryParse<TicketStatus>(filterStatus, true, out var status))
        {
            tickets = tickets.Where(t => t.Status == status).ToList();
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            tickets = tickets.Where(t => 
                t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.TicketNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        var model = new TicketListViewModel
        {
            Tickets = tickets,
            FilterStatus = filterStatus,
            SearchTerm = searchTerm,
            TotalCount = tickets.Count
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Assign(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            SetErrorMessage(ticketResult.ErrorMessage);
            return RedirectToAction(nameof(Index));
        }

        var supportStaff = await _userManager.GetUsersInRoleAsync("Support");
        
        var model = new AssignTicketViewModel
        {
            TicketId = id,
            TicketNumber = ticketResult.Data.TicketNumber,
            Title = ticketResult.Data.Title,
            AvailableStaff = supportStaff.ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var supportStaff = await _userManager.GetUsersInRoleAsync("Support");
            model.AvailableStaff = supportStaff.ToList();
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var result = await _ticketService.AssignTicketAsync(model.TicketId, model.AssignToUserId, currentUserId);

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket assigned successfully.");
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        SetErrorMessage(result.ErrorMessage);
        var staff = await _userManager.GetUsersInRoleAsync("Support");
        model.AvailableStaff = staff.ToList();
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            SetErrorMessage(ticketResult.ErrorMessage);
            return RedirectToAction(nameof(Index));
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
            CanAssign = true,
            CanEdit = true,
            CanResolve = false,
            CanClose = ticketResult.Data.Status == TicketStatus.Resolved,
            CanReopen = ticketResult.Data.Status == TicketStatus.Closed
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.TicketId });
        }

        var currentUserId = GetCurrentUserId();
        var result = await _ticketService.UpdateTicketStatusAsync(
            model.TicketId, 
            model.NewStatus, 
            currentUserId, 
            model.Notes
        );

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket status updated successfully.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = model.TicketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int ticketId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _ticketService.CloseTicketAsync(ticketId, currentUserId);

        if (result.IsSuccess)
        {
            SetSuccessMessage("Ticket closed successfully.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    public async Task<IActionResult> Unassigned()
    {
        var result = await _ticketService.GetUnassignedTicketsAsync();
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage);
            return View(new List<Ticket>());
        }

        return View(result.Data);
    }

    public async Task<IActionResult> Statistics()
    {
        var statsResult = await _ticketService.GetTicketStatisticsAsync();
        if (!statsResult.IsSuccess)
        {
            TempData["Error"] = statsResult.ErrorMessage;
            return View(new TicketStatistics());
        }

        return View(statsResult.Data);
    }
}