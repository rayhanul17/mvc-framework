using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Models;

namespace DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class DashboardController : Controller
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
        var userId = _userManager.GetUserId(User);
        var model = new DashboardViewModel();

        // Get statistics
        var statsResult = await _ticketService.GetTicketStatisticsAsync(userId);
        if (statsResult.IsSuccess)
        {
            model.Statistics = statsResult.Data;
        }

        // Get recent tickets based on role
        if (User.IsInRole("Customer"))
        {
            // Get customer's own tickets
            var ticketsResult = await _ticketService.GetTicketsAsync(userId);
            if (ticketsResult.IsSuccess)
            {
                model.MyTickets = ticketsResult.Data.Take(10).ToList();
            }
        }
        else if (User.IsInRole("Support"))
        {
            // Get assigned tickets
            var assignedResult = await _ticketService.GetAssignedTicketsAsync(userId);
            if (assignedResult.IsSuccess)
            {
                model.MyTickets = assignedResult.Data.Take(10).ToList();
            }
        }
        else if (User.IsInRole("SupportManager"))
        {
            // Get all recent tickets
            var allTicketsResult = await _ticketService.GetTicketsAsync();
            if (allTicketsResult.IsSuccess)
            {
                model.RecentTickets = allTicketsResult.Data.Take(10).ToList();
                
                // Get unassigned tickets
                var unassignedResult = await _ticketService.GetUnassignedTicketsAsync();
                if (unassignedResult.IsSuccess)
                {
                    model.MyTickets = unassignedResult.Data.Take(10).ToList();
                }
            }
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
        var userId = _userManager.GetUserId(User);
        var result = await _ticketService.GetUserNotificationsAsync(userId);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<TicketNotification>());
        }

        return View(result.Data);
    }
}