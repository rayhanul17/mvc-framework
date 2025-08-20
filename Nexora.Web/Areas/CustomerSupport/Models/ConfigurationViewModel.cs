using Nexora.Core.Entities;

namespace Nexora.Web.Areas.CustomerSupport.Models;

public class ConfigurationViewModel
{
    public List<RoleMappingViewModel> RoleMappings { get; set; } = new();
    public List<ApplicationRole> AvailableRoles { get; set; } = new();
    public List<string> TicketPriorities { get; set; } = new();
    public List<string> TicketStatuses { get; set; } = new();
}

public class RoleMappingViewModel
{
    public int Id { get; set; }
    public string CustomerServiceRole { get; set; } = string.Empty;
    public string AspNetRoleId { get; set; } = string.Empty;
    public string AspNetRoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}