using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Web.Controllers;
using System.Security.Claims;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class ManageTicketController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageTicketController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager)
    {
        _ticketService = ticketService;
        _userManager = userManager;
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
            SetErrorMessage(statsResult.ErrorMessage);
            return View(new TicketStatistics());
        }

        return View(statsResult.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickStatusUpdate(int ticketId, string newStatus)
    {
        if (!Enum.TryParse<TicketStatus>(newStatus, out var status))
        {
            SetErrorMessage("Invalid status provided.");
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = GetCurrentUserId();
        var result = await _ticketService.UpdateTicketStatusAsync(ticketId, status, currentUserId);

        if (result.IsSuccess)
        {
            SetSuccessMessage($"Ticket status updated to {status}.");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAssign([FromBody] BulkOperationViewModel model)
    {
        if (model.SelectedTicketIds == null || !model.SelectedTicketIds.Any())
        {
            return Json(new { success = false, message = "No tickets selected." });
        }

        if (string.IsNullOrEmpty(model.NewAssigneeId))
        {
            return Json(new { success = false, message = "Please select an assignee." });
        }

        var currentUserId = GetCurrentUserId();
        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var ticketId in model.SelectedTicketIds)
        {
            var result = await _ticketService.AssignTicketAsync(ticketId, model.NewAssigneeId, currentUserId);
            if (result.IsSuccess)
            {
                successCount++;
            }
            else
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: {result.ErrorMessage}");
            }
        }

        var message = $"Bulk assignment completed: {successCount} successful, {errorCount} failed.";
        if (errors.Any() && errors.Count <= 5)
        {
            message += " Errors: " + string.Join("; ", errors);
        }

        return Json(new { 
            success = errorCount == 0, 
            message = message,
            successCount = successCount,
            errorCount = errorCount
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkStatusUpdate([FromBody] BulkOperationViewModel model)
    {
        if (model.SelectedTicketIds == null || !model.SelectedTicketIds.Any())
        {
            return Json(new { success = false, message = "No tickets selected." });
        }

        if (!model.NewStatus.HasValue)
        {
            return Json(new { success = false, message = "Please select a status." });
        }

        var currentUserId = GetCurrentUserId();
        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var ticketId in model.SelectedTicketIds)
        {
            var result = await _ticketService.UpdateTicketStatusAsync(
                ticketId, 
                model.NewStatus.Value, 
                currentUserId, 
                model.Notes
            );
            
            if (result.IsSuccess)
            {
                successCount++;
            }
            else
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: {result.ErrorMessage}");
            }
        }

        var message = $"Bulk status update completed: {successCount} successful, {errorCount} failed.";
        if (errors.Any() && errors.Count <= 5)
        {
            message += " Errors: " + string.Join("; ", errors);
        }

        return Json(new { 
            success = errorCount == 0, 
            message = message,
            successCount = successCount,
            errorCount = errorCount
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkPriorityUpdate([FromBody] BulkOperationViewModel model)
    {
        if (model.SelectedTicketIds == null || !model.SelectedTicketIds.Any())
        {
            return Json(new { success = false, message = "No tickets selected." });
        }

        if (!model.NewPriority.HasValue)
        {
            return Json(new { success = false, message = "Please select a priority." });
        }

        var currentUserId = GetCurrentUserId();
        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var ticketId in model.SelectedTicketIds)
        {
            var ticketResult = await _ticketService.GetTicketByIdAsync(ticketId);
            if (!ticketResult.IsSuccess)
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: Not found");
                continue;
            }

            var ticket = ticketResult.Data;
            var oldPriority = ticket.Priority;
            ticket.Priority = model.NewPriority.Value;
            ticket.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _ticketService.UpdateTicketAsync(ticket);
            if (updateResult.IsSuccess)
            {
                await _ticketService.AddHistoryAsync(
                    ticketId, 
                    "Priority Changed", 
                    currentUserId, 
                    $"Priority changed from {oldPriority} to {model.NewPriority.Value}",
                    oldPriority.ToString(),
                    model.NewPriority.Value.ToString()
                );
                
                if (model.SendNotification)
                {
                    await _ticketService.CreateNotificationAsync(
                        ticketId,
                        ticket.CustomerId,
                        "Ticket Priority Updated",
                        $"Priority of ticket {ticket.TicketNumber} has been changed to {model.NewPriority.Value}",
                        NotificationType.StatusChange
                    );
                }
                
                successCount++;
            }
            else
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: {updateResult.ErrorMessage}");
            }
        }

        var message = $"Bulk priority update completed: {successCount} successful, {errorCount} failed.";
        if (errors.Any() && errors.Count <= 5)
        {
            message += " Errors: " + string.Join("; ", errors);
        }

        return Json(new { 
            success = errorCount == 0, 
            message = message,
            successCount = successCount,
            errorCount = errorCount
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableAgents()
    {
        var agents = await _userManager.GetUsersInRoleAsync("CustomerSupportAgent");
        var managers = await _userManager.GetUsersInRoleAsync("CustomerSupportManager");
        var admins = await _userManager.GetUsersInRoleAsync("CustomerSupportAdmin");
        
        var allStaff = agents.Concat(managers).Concat(admins)
            .GroupBy(u => u.Id)
            .Select(g => g.First())
            .Select(u => new 
            {
                id = u.Id,
                name = u.FullName,
                email = u.Email
            })
            .OrderBy(u => u.name)
            .ToList();

        return Json(allStaff);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkCategoryUpdate([FromBody] BulkOperationViewModel model)
    {
        if (model.SelectedTicketIds == null || !model.SelectedTicketIds.Any())
        {
            return Json(new { success = false, message = "No tickets selected." });
        }

        if (string.IsNullOrEmpty(model.NewCategory))
        {
            return Json(new { success = false, message = "Please specify a category." });
        }

        var currentUserId = GetCurrentUserId();
        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var ticketId in model.SelectedTicketIds)
        {
            var ticketResult = await _ticketService.GetTicketByIdAsync(ticketId);
            if (!ticketResult.IsSuccess)
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: Not found");
                continue;
            }

            var ticket = ticketResult.Data;
            var oldCategory = ticket.Category;
            ticket.Category = model.NewCategory;
            ticket.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _ticketService.UpdateTicketAsync(ticket);
            if (updateResult.IsSuccess)
            {
                await _ticketService.AddHistoryAsync(
                    ticketId, 
                    "Category Changed", 
                    currentUserId, 
                    $"Category changed from {oldCategory ?? "None"} to {model.NewCategory}",
                    oldCategory,
                    model.NewCategory
                );
                
                successCount++;
            }
            else
            {
                errorCount++;
                errors.Add($"Ticket {ticketId}: {updateResult.ErrorMessage}");
            }
        }

        var message = $"Bulk category update completed: {successCount} successful, {errorCount} failed.";
        if (errors.Any() && errors.Count <= 5)
        {
            message += " Errors: " + string.Join("; ", errors);
        }

        return Json(new { 
            success = errorCount == 0, 
            message = message,
            successCount = successCount,
            errorCount = errorCount
        });
    }
}