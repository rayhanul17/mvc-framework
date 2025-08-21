using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Web.Controllers;
using System.Security.Claims;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _ticketService = ticketService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = await GetUserCustomerServiceRoles();
            var model = await BuildDashboardViewModel(userId, userRoles);
            
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            SetErrorMessage("Error loading dashboard. Please try again.");
            return View(new DashboardViewModel());
        }
    }

    private async Task<DashboardViewModel> BuildDashboardViewModel(string userId, List<string> userRoles)
    {
        var model = new DashboardViewModel();
        var isAdmin = userRoles.Contains("CustomerSupportAdmin");
        var isManager = userRoles.Contains("CustomerSupportManager");
        var isAgent = userRoles.Contains("CustomerSupportAgent");
        
        // Get all tickets based on user permissions
        var ticketsQuery = _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .AsQueryable();

        // Apply role-based filtering
        if (!isAdmin && !isManager)
        {
            if (isAgent)
            {
                ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == userId || t.CustomerId == userId);
            }
            else
            {
                ticketsQuery = ticketsQuery.Where(t => t.CustomerId == userId);
            }
        }

        var tickets = await ticketsQuery.ToListAsync();
        
        // Build statistics
        model.Statistics = BuildStatistics(tickets);
        
        // Recent tickets (last 10 created)
        model.RecentTickets = tickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToList();

        // My tickets (assigned to current user or created by user)
        model.MyTickets = tickets
            .Where(t => t.AssignedToId == userId || t.CustomerId == userId)
            .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Take(10)
            .ToList();

        // Unassigned tickets (for admins and managers)
        if (isAdmin || isManager)
        {
            model.UnassignedTickets = tickets
                .Where(t => t.AssignedToId == null)
                .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedAt)
                .Take(10)
                .ToList();
        }

        // Get recent notifications
        model.RecentNotifications = await _context.TicketNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToListAsync();

        model.UnreadNotificationCount = await _context.TicketNotifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        // Additional metrics
        var today = DateTime.Today;
        var tickets_today = tickets.Where(t => t.CreatedAt.Date == today);
        var resolved_today = tickets.Where(t => t.ResolvedAt?.Date == today);

        model.TicketsCreatedToday = tickets_today.Count();
        model.TicketsResolvedToday = resolved_today.Count();
        model.OverdueTickets = tickets.Count(t => t.DueDate.HasValue && 
                                                 t.DueDate.Value < DateTime.Now && 
                                                 t.Status != TicketStatus.Resolved && 
                                                 t.Status != TicketStatus.Closed);
        
        model.HighPriorityTickets = tickets.Count(t => t.Priority >= TicketPriority.High && 
                                                      t.Status != TicketStatus.Resolved && 
                                                      t.Status != TicketStatus.Closed);

        // Calculate response times
        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue).ToList();
        if (resolvedTickets.Any())
        {
            model.AverageResolutionTime = resolvedTickets
                .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
        }

        // Get team performance (for managers and admins)
        if (isAdmin || isManager)
        {
            model.TopPerformers = await GetTopPerformers();
        }

        // Category breakdown
        model.CategoryBreakdown = GetCategoryStats(tickets);

        // Weekly trends
        model.WeeklyTrends = GetWeeklyTrends(tickets);

        return model;
    }

    private TicketStatistics BuildStatistics(List<Ticket> tickets)
    {
        var stats = new TicketStatistics
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open || 
                                           t.Status == TicketStatus.New || 
                                           t.Status == TicketStatus.InProgress),
            ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
            ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
            UnassignedTickets = tickets.Count(t => t.AssignedToId == null),
            OverdueTickets = tickets.Count(t => t.DueDate.HasValue && 
                                              t.DueDate.Value < DateTime.Now && 
                                              t.Status != TicketStatus.Resolved && 
                                              t.Status != TicketStatus.Closed)
        };

        // Calculate average resolution time
        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue).ToList();
        if (resolvedTickets.Any())
        {
            stats.AverageResolutionHours = resolvedTickets
                .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
        }

        // Group by priority
        stats.TicketsByPriority = tickets
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by category
        stats.TicketsByCategory = tickets
            .Where(t => !string.IsNullOrEmpty(t.Category))
            .GroupBy(t => t.Category!)
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    private async Task<List<AgentPerformance>> GetTopPerformers()
    {
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        
        var agents = await _context.Users
            .Where(u => _context.UserRoles
                .Any(ur => ur.UserId == u.Id && 
                          _context.Roles.Any(r => r.Id == ur.RoleId && 
                                                 (r.Name == "CustomerSupportAgent" || 
                                                  r.Name == "CustomerSupportManager"))))
            .Select(u => new AgentPerformance
            {
                AgentId = u.Id,
                AgentName = u.FullName,
                Email = u.Email,
                TicketsAssigned = _context.Tickets.Count(t => t.AssignedToId == u.Id && t.CreatedAt >= thirtyDaysAgo),
                TicketsResolved = _context.Tickets.Count(t => t.AssignedToId == u.Id && 
                                                              t.ResolvedAt.HasValue && 
                                                              t.ResolvedAt.Value >= thirtyDaysAgo),
                OpenTickets = _context.Tickets.Count(t => t.AssignedToId == u.Id && 
                                                         t.Status != TicketStatus.Resolved && 
                                                         t.Status != TicketStatus.Closed),
                LastActivity = _context.TicketComments
                    .Where(c => c.UserId == u.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => c.CreatedAt)
                    .FirstOrDefault(),
                IsOnline = false // Would be updated by real-time presence system
            })
            .ToListAsync();

        // Calculate average resolution time for each agent
        foreach (var agent in agents)
        {
            var resolvedTickets = await _context.Tickets
                .Where(t => t.AssignedToId == agent.AgentId && 
                           t.ResolvedAt.HasValue && 
                           t.ResolvedAt.Value >= thirtyDaysAgo)
                .ToListAsync();

            if (resolvedTickets.Any())
            {
                agent.AverageResolutionTime = resolvedTickets
                    .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
            }

            // Placeholder for customer satisfaction - would come from surveys
            agent.CustomerSatisfactionScore = new Random().NextDouble() * 2 + 3; // 3-5 range
        }

        return agents.OrderByDescending(a => a.TicketsResolved).Take(5).ToList();
    }

    private List<CategoryStats> GetCategoryStats(List<Ticket> tickets)
    {
        return tickets
            .Where(t => !string.IsNullOrEmpty(t.Category))
            .GroupBy(t => t.Category!)
            .Select(g => new CategoryStats
            {
                Category = g.Key,
                TicketCount = g.Count(),
                ResolvedCount = g.Count(t => t.Status == TicketStatus.Resolved),
                ResolutionRate = g.Any() ? (double)g.Count(t => t.Status == TicketStatus.Resolved) / g.Count() * 100 : 0,
                AverageResolutionTime = g.Where(t => t.ResolvedAt.HasValue).Any() ? 
                    g.Where(t => t.ResolvedAt.HasValue).Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours) : 0,
                MostCommonPriority = g.GroupBy(t => t.Priority)
                    .OrderByDescending(pg => pg.Count())
                    .First().Key
            })
            .OrderByDescending(c => c.TicketCount)
            .ToList();
    }

    private List<TrendData> GetWeeklyTrends(List<Ticket> tickets)
    {
        var sevenDaysAgo = DateTime.Now.Date.AddDays(-7);
        var trends = new List<TrendData>();

        for (int i = 0; i < 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            var dayTickets = tickets.Where(t => t.CreatedAt.Date == date);
            var dayResolved = tickets.Where(t => t.ResolvedAt?.Date == date);

            trends.Add(new TrendData
            {
                Date = date,
                TicketsCreated = dayTickets.Count(),
                TicketsResolved = dayResolved.Count(),
                TicketsClosed = tickets.Count(t => t.ClosedAt?.Date == date),
                AverageResolutionTime = dayResolved.Any() ? 
                    dayResolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours) : 0
            });
        }

        return trends;
    }

    private async Task<List<string>> GetUserCustomerServiceRoles()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return new List<string>();
        
        // Return all user roles directly without filtering
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        try
        {
            var notification = await _context.TicketNotifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Notification not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return Json(new { success = false, message = "Error updating notification" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _context.TicketNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = notifications.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Json(new { success = false, message = "Error updating notifications" });
        }
    }

    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = await GetUserCustomerServiceRoles();
            
            var ticketsQuery = _context.Tickets.AsQueryable();
            
            // Apply role-based filtering
            if (!userRoles.Contains("CustomerSupportAdmin") && !userRoles.Contains("CustomerSupportManager"))
            {
                if (userRoles.Contains("CustomerSupportAgent"))
                {
                    ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == userId || t.CustomerId == userId);
                }
                else
                {
                    ticketsQuery = ticketsQuery.Where(t => t.CustomerId == userId);
                }
            }

            var stats = new
            {
                totalTickets = await ticketsQuery.CountAsync(),
                openTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Open || 
                                                               t.Status == TicketStatus.New || 
                                                               t.Status == TicketStatus.InProgress),
                resolvedTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Resolved),
                unassignedTickets = await ticketsQuery.CountAsync(t => t.AssignedToId == null),
                overdueTickets = await ticketsQuery.CountAsync(t => t.DueDate.HasValue && 
                                                                   t.DueDate.Value < DateTime.Now && 
                                                                   t.Status != TicketStatus.Resolved && 
                                                                   t.Status != TicketStatus.Closed),
                myTickets = await ticketsQuery.CountAsync(t => t.AssignedToId == userId || t.CustomerId == userId)
            };

            return Json(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return Json(new { error = "Failed to load statistics" });
        }
    }

    public async Task<IActionResult> GetRecentActivity()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = await GetUserCustomerServiceRoles();

            // Get recent comments and ticket updates
            var recentComments = await _context.TicketComments
                .Include(c => c.Ticket)
                .Include(c => c.User)
                .Where(c => c.CreatedAt >= DateTime.Now.AddHours(-24))
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new
                {
                    id = c.Id,
                    ticketId = c.TicketId,
                    ticketNumber = c.Ticket.TicketNumber ?? $"T{c.Ticket.Id:D6}",
                    ticketTitle = c.Ticket.Title,
                    comment = c.Comment.Length > 100 ? c.Comment.Substring(0, 100) + "..." : c.Comment,
                    userName = c.User.FullName,
                    createdAt = c.CreatedAt,
                    isInternal = c.IsInternal,
                    isSystemGenerated = c.IsSystemGenerated
                })
                .ToListAsync();

            return Json(recentComments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity");
            return Json(new { error = "Failed to load recent activity" });
        }
    }
}