using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Services;
using Nexora.Core.Entities;
using System.Linq;

namespace Nexora.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
        var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

        ViewBag.UnreadCount = unreadCount;
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _notificationService.MarkAsReadAsync(id, user.Id);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _notificationService.MarkAllAsReadAsync(user.Id);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { count = 0 });

        var count = await _notificationService.GetUnreadCountAsync(user.Id);
        return Json(new { count });
    }
    
    [HttpGet]
    [Route("api/notifications/recent")]
    public async Task<IActionResult> GetRecentNotifications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new List<object>());

        var notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
        var recentNotifications = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type.ToString().ToLower(),
                isRead = n.IsRead,
                createdAt = n.CreatedAt.ToString("o"),
                url = GetNotificationUrl(n)
            })
            .ToList();

        return Json(recentNotifications);
    }
    
    [HttpPost]
    [Route("api/notifications/{id}/read")]
    public async Task<IActionResult> MarkAsReadApi(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _notificationService.MarkAsReadAsync(id, user.Id);
        return Ok();
    }
    
    private string? GetNotificationUrl(TicketNotification notification)
    {
        // If it's a ticket-related notification and has a TicketId, return the ticket URL
        if (notification.TicketId.HasValue)
        {
            return Url.Action("Details", "Tickets", new { id = notification.TicketId.Value });
        }
        
        // For general notifications, return null or a specific URL based on type
        return null;
    }
}