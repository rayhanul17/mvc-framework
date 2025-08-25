using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class NotificationSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Check if notifications already exist
        if (await context.TicketNotifications.AnyAsync())
            return;

        // Get all users to send welcome notifications
        var users = await userManager.Users.ToListAsync();
        
        // If no users exist yet, return (will be called again after users are created)
        if (!users.Any())
            return;

        var notifications = new List<TicketNotification>();

        foreach (var user in users)
        {
            // Welcome notification for each user
            notifications.Add(new TicketNotification
            {
                UserId = user.Id,
                Title = "Welcome to Our Platform!",
                Message = $"Hi {user.FullName}, welcome aboard! We're excited to have you join our community. Explore the features and let us know if you need any help getting started.",
                Type = NotificationType.Info,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-7) // Set to 7 days ago
            });

            // System update notification
            notifications.Add(new TicketNotification
            {
                UserId = user.Id,
                Title = "System Update",
                Message = "We've updated our platform with new features and improvements. Check out the latest changes in the documentation.",
                Type = NotificationType.Info,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            });

            // Feature announcement
            notifications.Add(new TicketNotification
            {
                UserId = user.Id,
                Title = "New Feature: Dark Mode",
                Message = "You can now switch between light and dark themes! Click the theme toggle in the sidebar to try it out.",
                Type = NotificationType.Info,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });

            // Only for admin/superadmin users
            if (await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                notifications.Add(new TicketNotification
                {
                    UserId = user.Id,
                    Title = "Admin Dashboard Updated",
                    Message = "The admin dashboard has been enhanced with new analytics and reporting features. Visit the admin panel to explore.",
                    Type = NotificationType.Info,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });

                // Security notification
                notifications.Add(new TicketNotification
                {
                    UserId = user.Id,
                    Title = "Security Update",
                    Message = "A security patch has been applied to the system. All user sessions have been refreshed for safety.",
                    Type = NotificationType.Escalation,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-12)
                });
            }

            // Only for regular users (not admin/superadmin)
            if (await userManager.IsInRoleAsync(user, "User"))
            {
                notifications.Add(new TicketNotification
                {
                    UserId = user.Id,
                    Title = "Complete Your Profile",
                    Message = "Don't forget to complete your profile! Add a profile picture and description to personalize your account.",
                    Type = NotificationType.Info,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                });
            }

            // Recent notification for all
            notifications.Add(new TicketNotification
            {
                UserId = user.Id,
                Title = "Maintenance Schedule",
                Message = "Scheduled maintenance will occur this weekend from 2 AM to 4 AM UTC. The system may be temporarily unavailable.",
                Type = NotificationType.DueDate,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            });
        }

        // Add some read notifications for variety
        foreach (var user in users.Take(2)) // First two users
        {
            notifications.Add(new TicketNotification
            {
                UserId = user.Id,
                Title = "Previous Announcement",
                Message = "This is an older notification that has been read.",
                Type = NotificationType.Info,
                IsRead = true,
                ReadAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });
        }

        if (notifications.Any())
        {
            await context.TicketNotifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Seeded {notifications.Count} notifications for {users.Count} users");
        }
        else
        {
            Console.WriteLine("No notifications to seed");
        }
    }
}