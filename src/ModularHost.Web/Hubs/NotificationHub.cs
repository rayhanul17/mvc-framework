using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, string title, string message)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendNotificationToRole(string roleName, string title, string message)
        {
            await Clients.Group($"role_{roleName}").SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendNotificationToAll(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message);
        }
    }
}