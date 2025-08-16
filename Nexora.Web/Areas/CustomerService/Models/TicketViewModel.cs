using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Core.Entities;

namespace Nexora.Web.Areas.CustomerService.Models;

public class TicketListViewModel
{
    public List<TicketItemViewModel> Tickets { get; set; } = new();
    public TicketFilterModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class TicketItemViewModel
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
    public DateTime? DueDate { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TicketStatus.Closed && Status != TicketStatus.Resolved;
}

public class TicketFilterModel
{
    public string? SearchTerm { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? Category { get; set; }
    public string? AssignedToId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class CreateTicketViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Priority")]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    [StringLength(100)]
    [Display(Name = "Category")]
    public string? Category { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Sub-Category")]
    public string? SubCategory { get; set; }
    
    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }
    
    [Display(Name = "Assign To")]
    public string? AssignedToUserId { get; set; }
    
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }
    
    public List<SelectListItem> AvailableAgents { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
}

public class EditTicketViewModel : CreateTicketViewModel
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    public TicketStatus Status { get; set; }
    
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<TicketAttachment> ExistingAttachments { get; set; } = new();
}

public class TicketDetailsViewModel
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string? CreatedByEmail { get; set; }
    
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToEmail { get; set; }
    
    public List<TicketCommentViewModel> Comments { get; set; } = new();
    public List<TicketAttachment> Attachments { get; set; } = new();
    public List<TicketHistoryViewModel> History { get; set; } = new();
    
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanAssign { get; set; }
    public bool CanChangeStatus { get; set; }
    public bool CanComment { get; set; }
}

public class TicketCommentViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public bool IsSystemGenerated { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public List<TicketAttachment> Attachments { get; set; } = new();
}

public class TicketHistoryViewModel
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class AddCommentViewModel
{
    public int TicketId { get; set; }
    
    [Required]
    [StringLength(2000)]
    [Display(Name = "Comment")]
    public string Content { get; set; } = string.Empty;
    
    [Display(Name = "Internal Note")]
    public bool IsInternal { get; set; }
    
    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }
}

public class AssignTicketViewModel
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Assign To")]
    public string AssignToUserId { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Display(Name = "Assignment Note")]
    public string? AssignmentNote { get; set; }
    
    public List<SelectListItem> AvailableAgents { get; set; } = new();
}

public class UpdateStatusViewModel
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "New Status")]
    public TicketStatus NewStatus { get; set; }
    
    [StringLength(500)]
    [Display(Name = "Status Note")]
    public string? StatusNote { get; set; }
    
    public TicketStatus CurrentStatus { get; set; }
    public List<SelectListItem> AvailableStatuses { get; set; } = new();
}