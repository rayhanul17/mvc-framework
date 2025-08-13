using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Core.Entities;
using Nexora.Application.Interfaces;

namespace Nexora.Web.Areas.CustomerSupport.Models;

public class CreateTicketViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Category")]
    public string Category { get; set; } = "General";
    
    [Display(Name = "Priority")]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }
}

public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; } = null!;
    public List<TicketComment> Comments { get; set; } = new();
    public List<TicketAttachment> Attachments { get; set; } = new();
    public List<TicketHistory> History { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanAssign { get; set; }
    public bool CanResolve { get; set; }
    public bool CanClose { get; set; }
    public bool CanReopen { get; set; }
}

public class AddCommentViewModel
{
    public int TicketId { get; set; }
    
    [Required]
    [Display(Name = "Comment")]
    public string Comment { get; set; } = string.Empty;
    
    [Display(Name = "Internal Note")]
    public bool IsInternal { get; set; }
    
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }
}

public class AssignTicketViewModel
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Assign To")]
    public string AssignToUserId { get; set; } = string.Empty;
    
    public List<ApplicationUser> AvailableStaff { get; set; } = new();
}

public class UpdateStatusViewModel
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketStatus CurrentStatus { get; set; }
    
    [Required]
    [Display(Name = "New Status")]
    public TicketStatus NewStatus { get; set; }
    
    [Display(Name = "Notes")]
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ResolveTicketViewModel  
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Resolution Notes")]
    [StringLength(500)]
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class TicketListViewModel
{
    public List<Ticket> Tickets { get; set; } = new();
    public string FilterStatus { get; set; } = "all";
    public string FilterPriority { get; set; } = "all";
    public string SearchTerm { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    // Additional properties for enhanced functionality
    public string FilterAssignment { get; set; } = "all";
    public string FilterCategory { get; set; } = "all";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class DashboardViewModel
{
    public string WelcomeMessage { get; set; } = "Welcome to Customer Support Dashboard";
    public TicketStatistics Statistics { get; set; } = new();
    public List<Ticket> RecentTickets { get; set; } = new();
    public List<Ticket> MyTickets { get; set; } = new();
    public List<Ticket> UnassignedTickets { get; set; } = new();
    public List<TicketNotification> RecentNotifications { get; set; } = new();
    public int UnreadNotificationCount { get; set; }
    
    // Performance metrics
    public double AverageResolutionTime { get; set; }
    public double FirstResponseTime { get; set; }
    public int TicketsCreatedToday { get; set; }
    public int TicketsResolvedToday { get; set; }
    public int OverdueTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    
    // Team performance
    public List<AgentPerformance> TopPerformers { get; set; } = new();
    public List<CategoryStats> CategoryBreakdown { get; set; } = new();
    public List<TrendData> WeeklyTrends { get; set; } = new();
}

// Supporting classes for enhanced functionality
public class AgentPerformance
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TicketsAssigned { get; set; }
    public int TicketsResolved { get; set; }
    public double AverageResolutionTime { get; set; }
    public double CustomerSatisfactionScore { get; set; }
    public int OpenTickets { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsOnline { get; set; }
}

public class CategoryStats
{
    public string Category { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int ResolvedCount { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTime { get; set; }
    public TicketPriority MostCommonPriority { get; set; }
}

public class TrendData
{
    public DateTime Date { get; set; }
    public int TicketsCreated { get; set; }
    public int TicketsResolved { get; set; }
    public int TicketsClosed { get; set; }
    public double AverageResolutionTime { get; set; }
}

// Enhanced ticket item view model for lists
public class TicketItemViewModel
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public string? AssignedToId { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && 
                           Status != TicketStatus.Resolved && Status != TicketStatus.Closed;
    public TimeSpan Age => DateTime.Now - CreatedAt;
    public string AgeDisplay => Age.TotalDays >= 1 ? $"{(int)Age.TotalDays}d {Age.Hours}h" : 
                               Age.TotalHours >= 1 ? $"{(int)Age.TotalHours}h {Age.Minutes}m" : 
                               $"{Age.Minutes}m";
}

// Filter model for advanced search
public class TicketFilterModel
{
    public string? SearchTerm { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? Category { get; set; }
    public string? AssignedToId { get; set; }
    public string? CreatedById { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? HasAttachments { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    
    // Advanced filters
    public int? MinComments { get; set; }
    public int? MaxComments { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }
}

// Bulk operations view model
public class BulkOperationViewModel
{
    public List<int> SelectedTicketIds { get; set; } = new();
    public string Operation { get; set; } = string.Empty; // assign, status, priority, category
    public string? NewAssigneeId { get; set; }
    public TicketStatus? NewStatus { get; set; }
    public TicketPriority? NewPriority { get; set; }
    public string? NewCategory { get; set; }
    public string? Notes { get; set; }
    public bool SendNotification { get; set; } = true;
}

// Comprehensive ticket list view model
public class TicketListPageViewModel
{
    public List<TicketItemViewModel> Tickets { get; set; } = new();
    public TicketFilterModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    // Dropdown data for filters
    public List<SelectListItem> AvailableAgents { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Customers { get; set; } = new();
    
    // Quick stats for the current filter
    public int OpenCount { get; set; }
    public int ResolvedCount { get; set; }
    public int OverdueCount { get; set; }
    public int UnassignedCount { get; set; }
}

// SLA and escalation models
public class SLAViewModel
{
    public int TicketId { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstResponseDue { get; set; }
    public DateTime? ResolutionDue { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsFirstResponseOverdue => FirstResponseDue.HasValue && DateTime.Now > FirstResponseDue.Value && !FirstResponseAt.HasValue;
    public bool IsResolutionOverdue => ResolutionDue.HasValue && DateTime.Now > ResolutionDue.Value && !ResolvedAt.HasValue;
    public TimeSpan? TimeToFirstResponse => FirstResponseAt.HasValue ? FirstResponseAt.Value - CreatedAt : null;
    public TimeSpan? TimeToResolution => ResolvedAt.HasValue ? ResolvedAt.Value - CreatedAt : null;
}

// Knowledge base integration
public class KnowledgeBaseArticleViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsPublic { get; set; }
}

// Customer portal models
public class CustomerTicketViewModel
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<CustomerCommentViewModel> Comments { get; set; } = new();
    public List<TicketAttachment> Attachments { get; set; } = new();
    public bool CanAddComment { get; set; }
    public bool CanClose { get; set; }
    public string? AssignedAgentName { get; set; }
}

public class CustomerCommentViewModel
{
    public int Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsFromAgent { get; set; }
    public bool IsSystemGenerated { get; set; }
    public List<TicketAttachment> Attachments { get; set; } = new();
}

// Reporting models
public class TicketReportViewModel
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public TicketReportData Data { get; set; } = new();
}

public class TicketReportData
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public double AverageResolutionTime { get; set; }
    public double AverageFirstResponseTime { get; set; }
    public double CustomerSatisfactionScore { get; set; }
    public Dictionary<TicketStatus, int> TicketsByStatus { get; set; } = new();
    public Dictionary<TicketPriority, int> TicketsByPriority { get; set; } = new();
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
    public Dictionary<string, int> TicketsByAgent { get; set; } = new();
    public List<TrendData> DailyTrends { get; set; } = new();
    public List<AgentPerformance> AgentPerformance { get; set; } = new();
}