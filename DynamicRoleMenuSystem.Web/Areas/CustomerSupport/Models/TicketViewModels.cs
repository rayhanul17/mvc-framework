using System.ComponentModel.DataAnnotations;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Application.Interfaces;

namespace DynamicRoleMenuSystem.Web.Areas.CustomerSupport.Models;

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
}

public class DashboardViewModel
{
    public TicketStatistics Statistics { get; set; } = new();
    public List<Ticket> RecentTickets { get; set; } = new();
    public List<Ticket> MyTickets { get; set; } = new();
    public List<Ticket> UnassignedTickets { get; set; } = new();
    public List<TicketNotification> RecentNotifications { get; set; } = new();
    public int UnreadNotificationCount { get; set; }
}