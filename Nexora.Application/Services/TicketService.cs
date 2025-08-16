using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Ticket>> CreateTicketAsync(Ticket ticket)
    {
        try
        {
            ticket.TicketNumber = await GenerateTicketNumberAsync();
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.New;
            
            await _unitOfWork.Repository<Ticket>().AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
            
            // Create notification for managers
            await CreateNotificationForManagersAsync(ticket, "New Ticket Created", 
                $"A new ticket #{ticket.TicketNumber} has been created", NotificationType.NewTicket);
            
            // Add history entry
            await AddHistoryAsync(ticket.Id, "Ticket Created", ticket.CustomerId, 
                $"Ticket created with priority {ticket.Priority}");
            
            return Result<Ticket>.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket");
            return Result<Ticket>.Failure($"Error creating ticket: {ex.Message}");
        }
    }

    public async Task<Result<Ticket>> GetTicketByIdAsync(int id)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedTo)
                .Include(t => t.AssignedBy)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (ticket == null)
                return Result<Ticket>.Failure("Ticket not found");
                
            return Result<Ticket>.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket by id: {Id}", id);
            return Result<Ticket>.Failure($"Error getting ticket: {ex.Message}");
        }
    }

    public async Task<Result<Ticket>> GetTicketByNumberAsync(string ticketNumber)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
                
            if (ticket == null)
                return Result<Ticket>.Failure("Ticket not found");
                
            return Result<Ticket>.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket by number: {Number}", ticketNumber);
            return Result<Ticket>.Failure($"Error getting ticket: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> GetTicketsAsync(string? userId = null, TicketStatus? status = null)
    {
        try
        {
            var query = _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedTo)
                .AsQueryable();
                
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(t => t.CustomerId == userId || t.AssignedToId == userId);
                
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);
                
            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets");
            return Result<List<Ticket>>.Failure($"Error getting tickets: {ex.Message}");
        }
    }

    public async Task<Result<Ticket>> UpdateTicketAsync(Ticket ticket)
    {
        try
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Ticket>().Update(ticket);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<Ticket>.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket");
            return Result<Ticket>.Failure($"Error updating ticket: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTicketAsync(int id)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(id);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            _unitOfWork.Repository<Ticket>().Remove(ticket);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ticket");
            return Result<bool>.Failure($"Error deleting ticket: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AssignTicketAsync(int ticketId, string assignToUserId, string assignedByUserId)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            var oldAssignee = ticket.AssignedToId;
            ticket.AssignedToId = assignToUserId;
            ticket.AssignedById = assignedByUserId;
            ticket.AssignedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.Assigned;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Repository<Ticket>().Update(ticket);
            
            // Add comment
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = assignedByUserId,
                Comment = $"Ticket assigned to support staff",
                Type = CommentType.Assignment,
                IsInternal = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<TicketComment>().AddAsync(comment);
            
            // Create notification for assigned user
            await CreateNotificationAsync(ticketId, assignToUserId, "Ticket Assigned", 
                $"Ticket #{ticket.TicketNumber} has been assigned to you", NotificationType.TicketAssigned);
            
            // Add history
            await AddHistoryAsync(ticketId, "Ticket Assigned", assignedByUserId, 
                $"Ticket assigned to support staff", oldAssignee, assignToUserId);
            
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning ticket");
            return Result<bool>.Failure($"Error assigning ticket: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> GetAssignedTicketsAsync(string userId)
    {
        try
        {
            var tickets = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Where(t => t.AssignedToId == userId && t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
                
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assigned tickets");
            return Result<List<Ticket>>.Failure($"Error getting assigned tickets: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> GetUnassignedTicketsAsync()
    {
        try
        {
            var tickets = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Where(t => t.AssignedToId == null && t.Status == TicketStatus.New)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
                
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unassigned tickets");
            return Result<List<Ticket>>.Failure($"Error getting unassigned tickets: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string userId, string? notes = null)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            var oldStatus = ticket.Status;
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            if (newStatus == TicketStatus.InProgress && oldStatus == TicketStatus.Assigned)
            {
                var responseTime = (DateTime.UtcNow - ticket.CreatedAt).TotalHours;
                ticket.ResponseTimeHours = (int)responseTime;
            }
            
            _unitOfWork.Repository<Ticket>().Update(ticket);
            
            // Add status change comment
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = notes ?? $"Status changed from {oldStatus} to {newStatus}",
                Type = CommentType.StatusChange,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<TicketComment>().AddAsync(comment);
            
            // Create notification
            await CreateNotificationAsync(ticketId, ticket.CustomerId, "Ticket Status Updated", 
                $"Your ticket #{ticket.TicketNumber} status has been updated to {newStatus}", 
                NotificationType.StatusChanged);
            
            // Add history
            await AddHistoryAsync(ticketId, "Status Changed", userId, notes, 
                oldStatus.ToString(), newStatus.ToString());
            
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket status");
            return Result<bool>.Failure($"Error updating ticket status: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ResolveTicketAsync(int ticketId, string resolutionNotes, string userId)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            ticket.Status = TicketStatus.Resolved;
            ticket.ResolutionNotes = resolutionNotes;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            var resolutionTime = (DateTime.UtcNow - ticket.CreatedAt).TotalHours;
            ticket.ResolutionTimeHours = (int)resolutionTime;
            
            _unitOfWork.Repository<Ticket>().Update(ticket);
            
            // Add resolution comment
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = resolutionNotes,
                Type = CommentType.Resolution,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<TicketComment>().AddAsync(comment);
            
            // Notify customer
            await CreateNotificationAsync(ticketId, ticket.CustomerId, "Ticket Resolved", 
                $"Your ticket #{ticket.TicketNumber} has been resolved", NotificationType.TicketResolved);
            
            // Add history
            await AddHistoryAsync(ticketId, "Ticket Resolved", userId, resolutionNotes);
            
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving ticket");
            return Result<bool>.Failure($"Error resolving ticket: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CloseTicketAsync(int ticketId, string userId)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Repository<Ticket>().Update(ticket);
            
            // Notify if needed
            await CreateNotificationAsync(ticketId, ticket.CustomerId, "Ticket Closed", 
                $"Your ticket #{ticket.TicketNumber} has been closed", NotificationType.TicketClosed);
            
            // Add history
            await AddHistoryAsync(ticketId, "Ticket Closed", userId);
            
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing ticket");
            return Result<bool>.Failure($"Error closing ticket: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ReopenTicketAsync(int ticketId, string reason, string userId)
    {
        try
        {
            var ticket = await _unitOfWork.Repository<Ticket>().GetByIdAsync(ticketId);
            if (ticket == null)
                return Result<bool>.Failure("Ticket not found");
                
            ticket.Status = TicketStatus.Reopened;
            ticket.ReopenedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Repository<Ticket>().Update(ticket);
            
            // Add reopen comment
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = reason,
                Type = CommentType.Reopened,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<TicketComment>().AddAsync(comment);
            
            // Notify assigned user if exists
            if (!string.IsNullOrEmpty(ticket.AssignedToId))
            {
                await CreateNotificationAsync(ticketId, ticket.AssignedToId, "Ticket Reopened", 
                    $"Ticket #{ticket.TicketNumber} has been reopened", NotificationType.TicketReopened);
            }
            
            // Add history
            await AddHistoryAsync(ticketId, "Ticket Reopened", userId, reason);
            
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening ticket");
            return Result<bool>.Failure($"Error reopening ticket: {ex.Message}");
        }
    }

    // Continue with remaining methods...
    
    public async Task<string> GenerateTicketNumberAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var lastTicket = await _unitOfWork.Repository<Ticket>()
            .GetQueryable()
            .Where(t => t.TicketNumber.StartsWith($"TKT{date}"))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();
            
        int sequence = 1;
        if (lastTicket != null)
        {
            var lastSequence = lastTicket.TicketNumber.Substring(11);
            if (int.TryParse(lastSequence, out int parsed))
                sequence = parsed + 1;
        }
        
        return $"TKT{date}{sequence:D4}";
    }

    // Helper methods
    private async Task CreateNotificationForManagersAsync(Ticket ticket, string title, string message, NotificationType type)
    {
        // This would ideally get all users with Manager role
        // For now, we'll skip implementation
        await Task.CompletedTask;
    }

    // Implement remaining interface methods...
    public async Task<Result<TicketComment>> AddCommentAsync(TicketComment comment)
    {
        try
        {
            comment.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<TicketComment>().AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<TicketComment>.Success(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return Result<TicketComment>.Failure($"Error adding comment: {ex.Message}");
        }
    }

    public async Task<Result<List<TicketComment>>> GetTicketCommentsAsync(int ticketId, bool includeInternal = false)
    {
        try
        {
            var query = _unitOfWork.Repository<TicketComment>()
                .GetQueryable()
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId);
                
            if (!includeInternal)
                query = query.Where(c => !c.IsInternal);
                
            var comments = await query.OrderBy(c => c.CreatedAt).ToListAsync();
            return Result<List<TicketComment>>.Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments");
            return Result<List<TicketComment>>.Failure($"Error getting comments: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CreateNotificationAsync(int ticketId, string userId, string title, string message, NotificationType type)
    {
        try
        {
            var notification = new TicketNotification
            {
                TicketId = ticketId,
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<TicketNotification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return Result<bool>.Failure($"Error creating notification: {ex.Message}");
        }
    }

    public async Task<Result<List<TicketNotification>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        try
        {
            var query = _unitOfWork.Repository<TicketNotification>()
                .GetQueryable()
                .Include(n => n.Ticket)
                .Where(n => n.UserId == userId);
                
            if (unreadOnly)
                query = query.Where(n => !n.IsRead);
                
            var notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
            return Result<List<TicketNotification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return Result<List<TicketNotification>>.Failure($"Error getting notifications: {ex.Message}");
        }
    }

    public async Task<Result<bool>> MarkNotificationAsReadAsync(int notificationId)
    {
        try
        {
            var notification = await _unitOfWork.Repository<TicketNotification>().GetByIdAsync(notificationId);
            if (notification == null)
                return Result<bool>.Failure("Notification not found");
                
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            _unitOfWork.Repository<TicketNotification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return Result<bool>.Failure($"Error marking notification as read: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUnreadNotificationCountAsync(string userId)
    {
        try
        {
            var count = await _unitOfWork.Repository<TicketNotification>()
                .GetQueryable()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
                
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return Result<int>.Failure($"Error getting unread notification count: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddHistoryAsync(int ticketId, string action, string userId, string? description = null, string? oldValue = null, string? newValue = null)
    {
        try
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                Action = action,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<TicketHistory>().AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding history");
            return Result<bool>.Failure($"Error adding history: {ex.Message}");
        }
    }

    public async Task<Result<List<TicketHistory>>> GetTicketHistoryAsync(int ticketId)
    {
        try
        {
            var history = await _unitOfWork.Repository<TicketHistory>()
                .GetQueryable()
                .Include(h => h.User)
                .Where(h => h.TicketId == ticketId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
                
            return Result<List<TicketHistory>>.Success(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket history");
            return Result<List<TicketHistory>>.Failure($"Error getting ticket history: {ex.Message}");
        }
    }

    // Implement remaining methods with basic implementations for now
    public async Task<Result<TicketAttachment>> AddAttachmentAsync(TicketAttachment attachment)
    {
        try
        {
            attachment.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<TicketAttachment>().AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();
            return Result<TicketAttachment>.Success(attachment);
        }
        catch (Exception ex)
        {
            return Result<TicketAttachment>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TicketAttachment>>> GetTicketAttachmentsAsync(int ticketId)
    {
        try
        {
            var attachments = await _unitOfWork.Repository<TicketAttachment>()
                .GetQueryable()
                .Where(a => a.TicketId == ticketId)
                .ToListAsync();
            return Result<List<TicketAttachment>>.Success(attachments);
        }
        catch (Exception ex)
        {
            return Result<List<TicketAttachment>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAttachmentAsync(int attachmentId)
    {
        try
        {
            var attachment = await _unitOfWork.Repository<TicketAttachment>().GetByIdAsync(attachmentId);
            if (attachment != null)
            {
                _unitOfWork.Repository<TicketAttachment>().Remove(attachment);
                await _unitOfWork.SaveChangesAsync();
            }
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<TicketStatistics>> GetTicketStatisticsAsync(string? userId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<Ticket>().GetQueryable();
            
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(t => t.CustomerId == userId || t.AssignedToId == userId);

            var tickets = await query.ToListAsync();
            
            var stats = new TicketStatistics
            {
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(t => t.Status == TicketStatus.New || t.Status == TicketStatus.Assigned || t.Status == TicketStatus.InProgress),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
                UnassignedTickets = tickets.Count(t => t.AssignedToId == null),
                AverageResolutionHours = tickets.Where(t => t.ResolutionTimeHours > 0).Select(t => (double)t.ResolutionTimeHours).DefaultIfEmpty(0).Average()
            };

            foreach (var priority in Enum.GetValues<TicketPriority>())
            {
                stats.TicketsByPriority[priority] = tickets.Count(t => t.Priority == priority);
            }

            foreach (var category in tickets.Select(t => t.Category).Distinct())
            {
                stats.TicketsByCategory[category] = tickets.Count(t => t.Category == category);
            }

            return Result<TicketStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<TicketStatistics>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> GetTicketsByPriorityAsync(TicketPriority priority)
    {
        try
        {
            var tickets = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Where(t => t.Priority == priority)
                .ToListAsync();
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            return Result<List<Ticket>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> GetOverdueTicketsAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-2); // Tickets older than 2 days without assignment
            var tickets = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Where(t => t.CreatedAt < cutoffDate && t.AssignedToId == null && t.Status == TicketStatus.New)
                .ToListAsync();
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            return Result<List<Ticket>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<Ticket>>> SearchTicketsAsync(string searchTerm)
    {
        try
        {
            var tickets = await _unitOfWork.Repository<Ticket>()
                .GetQueryable()
                .Include(t => t.Customer)
                .Where(t => t.Title.Contains(searchTerm) || 
                           t.Description.Contains(searchTerm) || 
                           t.TicketNumber.Contains(searchTerm))
                .ToListAsync();
            return Result<List<Ticket>>.Success(tickets);
        }
        catch (Exception ex)
        {
            return Result<List<Ticket>>.Failure($"Error: {ex.Message}");
        }
    }
}