using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface ITicketService
{
    // Ticket CRUD
    Task<Result<Ticket>> CreateTicketAsync(Ticket ticket);
    Task<Result<Ticket>> GetTicketByIdAsync(int id);
    Task<Result<Ticket>> GetTicketByNumberAsync(string ticketNumber);
    Task<Result<List<Ticket>>> GetTicketsAsync(string? userId = null, TicketStatus? status = null);
    Task<Result<Ticket>> UpdateTicketAsync(Ticket ticket);
    Task<Result<bool>> DeleteTicketAsync(int id);
    
    // Assignment
    Task<Result<bool>> AssignTicketAsync(int ticketId, string assignToUserId, string assignedByUserId);
    Task<Result<List<Ticket>>> GetAssignedTicketsAsync(string userId);
    Task<Result<List<Ticket>>> GetUnassignedTicketsAsync();
    
    // Status Management
    Task<Result<bool>> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string userId, string? notes = null);
    Task<Result<bool>> ResolveTicketAsync(int ticketId, string resolutionNotes, string userId);
    Task<Result<bool>> CloseTicketAsync(int ticketId, string userId);
    Task<Result<bool>> ReopenTicketAsync(int ticketId, string reason, string userId);
    
    // Comments
    Task<Result<TicketComment>> AddCommentAsync(TicketComment comment);
    Task<Result<List<TicketComment>>> GetTicketCommentsAsync(int ticketId, bool includeInternal = false);
    
    // Attachments
    Task<Result<TicketAttachment>> AddAttachmentAsync(TicketAttachment attachment);
    Task<Result<List<TicketAttachment>>> GetTicketAttachmentsAsync(int ticketId);
    Task<Result<bool>> DeleteAttachmentAsync(int attachmentId);
    
    // Notifications
    Task<Result<bool>> CreateNotificationAsync(int ticketId, string userId, string title, string message, NotificationType type);
    Task<Result<List<TicketNotification>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<Result<bool>> MarkNotificationAsReadAsync(int notificationId);
    Task<Result<int>> GetUnreadNotificationCountAsync(string userId);
    
    // History
    Task<Result<bool>> AddHistoryAsync(int ticketId, string action, string userId, string? description = null, string? oldValue = null, string? newValue = null);
    Task<Result<List<TicketHistory>>> GetTicketHistoryAsync(int ticketId);
    
    // Statistics
    Task<Result<TicketStatistics>> GetTicketStatisticsAsync(string? userId = null);
    Task<Result<List<Ticket>>> GetTicketsByPriorityAsync(TicketPriority priority);
    Task<Result<List<Ticket>>> GetOverdueTicketsAsync();
    
    // Search
    Task<Result<List<Ticket>>> SearchTicketsAsync(string searchTerm);
    
    // Ticket Number Generation
    Task<string> GenerateTicketNumberAsync();
}

public class TicketStatistics
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int UnassignedTickets { get; set; }
    public int OverdueTickets { get; set; }
    public double AverageResolutionHours { get; set; }
    public Dictionary<TicketPriority, int> TicketsByPriority { get; set; } = new();
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
}