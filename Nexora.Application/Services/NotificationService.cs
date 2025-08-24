using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;
using System.Collections.Concurrent;

namespace Nexora.Application.Services;

public interface INotificationService
{
    Task SendTicketNotificationAsync(string userId, string title, string message, NotificationType type);
    Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type);
    Task SendRealTimeNotificationAsync(string userId, object notification);
    Task<List<TicketNotification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;
    private static readonly ConcurrentDictionary<string, List<object>> _pendingNotifications = new();

    public NotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendTicketNotificationAsync(string userId, string title, string message, NotificationType type)
    {
        try
        {
            var notification = new TicketNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TicketNotification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(userId, new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type.ToString(),
                createdAt = notification.CreatedAt,
                isRead = false
            });

            _logger.LogInformation($"Notification sent to user {userId}: {title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to user {userId}");
        }
    }

    public async Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type)
    {
        var tasks = userIds.Select(userId => SendTicketNotificationAsync(userId, title, message, type));
        await Task.WhenAll(tasks);
    }

    public async Task SendRealTimeNotificationAsync(string userId, object notification)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
            
            // Store notification if user is offline
            if (!NotificationHub.IsUserOnline(userId))
            {
                if (!_pendingNotifications.ContainsKey(userId))
                {
                    _pendingNotifications[userId] = new List<object>();
                }
                _pendingNotifications[userId].Add(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending real-time notification to user {userId}");
        }
    }

    public Task<List<TicketNotification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = _unitOfWork.Repository<TicketNotification>()
            .GetQueryable()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return Task.FromResult(query.OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to last 50 notifications
            .ToList());
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Repository<TicketNotification>()
            .GetByIdAsync(notificationId);

        if (notification != null && notification.UserId == userId)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Repository<TicketNotification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = _unitOfWork.Repository<TicketNotification>()
            .GetQueryable()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Repository<TicketNotification>().Update(notification);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public Task<int> GetUnreadCountAsync(string userId)
    {
        var count = _unitOfWork.Repository<TicketNotification>()
            .GetQueryable()
            .Count(n => n.UserId == userId && !n.IsRead);
        return Task.FromResult(count);
    }

    public static List<object> GetPendingNotifications(string userId)
    {
        if (_pendingNotifications.TryRemove(userId, out var notifications))
        {
            return notifications;
        }
        return new List<object>();
    }
}

// SignalR Hub for real-time notifications
public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
            
            // Send any pending notifications
            var pendingNotifications = NotificationService.GetPendingNotifications(userId);
            foreach (var notification in pendingNotifications)
            {
                await Clients.Caller.SendAsync("ReceiveNotification", notification);
            }
            
            _logger.LogInformation($"User {userId} connected to notification hub");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.TryRemove(userId, out _);
            _logger.LogInformation($"User {userId} disconnected from notification hub");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public static bool IsUserOnline(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    public async Task SubscribeToTicket(int ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    public async Task UnsubscribeFromTicket(int ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }
}