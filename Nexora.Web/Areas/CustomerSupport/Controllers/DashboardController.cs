using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Web.Controllers;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _ticketService = ticketService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var model = new DashboardViewModel();

        // Get statistics
        var statsResult = await _ticketService.GetTicketStatisticsAsync(userId);
        if (statsResult.IsSuccess)
        {
            model.Statistics = statsResult.Data;
        }

        // Get tickets visible to the user based on their permissions
        // Try to get all tickets first (will be filtered by service based on permissions)
        var allTicketsResult = await _ticketService.GetTicketsAsync();
        if (allTicketsResult.IsSuccess && allTicketsResult.Data.Any())
        {
            model.RecentTickets = allTicketsResult.Data.Take(10).ToList();
        }
        
        // Get user's assigned tickets
        var assignedResult = await _ticketService.GetAssignedTicketsAsync(userId);
        if (assignedResult.IsSuccess && assignedResult.Data.Any())
        {
            model.MyTickets = assignedResult.Data.Take(10).ToList();
        }
        
        // If no assigned tickets, try to get user's own tickets
        if (!model.MyTickets.Any())
        {
            var myTicketsResult = await _ticketService.GetTicketsAsync(userId);
            if (myTicketsResult.IsSuccess)
            {
                model.MyTickets = myTicketsResult.Data.Take(10).ToList();
            }
        }
        
        // Get unassigned tickets if user has permission
        var unassignedResult = await _ticketService.GetUnassignedTicketsAsync();
        if (unassignedResult.IsSuccess && unassignedResult.Data.Any())
        {
            // Show unassigned tickets if user has access to them
            model.UnassignedTickets = unassignedResult.Data.Take(5).ToList();
        }

        // Get notifications
        var notificationsResult = await _ticketService.GetUserNotificationsAsync(userId, true);
        if (notificationsResult.IsSuccess)
        {
            model.RecentNotifications = notificationsResult.Data.Take(5).ToList();
            model.UnreadNotificationCount = notificationsResult.Data.Count;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        var result = await _ticketService.MarkNotificationAsReadAsync(id);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = result.ErrorMessage });
    }

    public async Task<IActionResult> Notifications()
    {
        var userId = GetCurrentUserId();
        var result = await _ticketService.GetUserNotificationsAsync(userId);
        
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage);
            return View(new List<TicketNotification>());
        }

        return View(result.Data);
    }
}