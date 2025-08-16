namespace Nexora.Web.Areas.CustomerService.Models;

public class CustomerServiceDashboardViewModel
{
    public string SiteName { get; set; } = "Nexora Framework";
    public string WelcomeMessage { get; set; } = "Welcome to Customer Service";
    
    // Role Mapping Statistics
    public int TotalRoleMappings { get; set; }
    public int ActiveRoleMappings { get; set; }
    public int InactiveRoleMappings { get; set; }
    
    // Ticket Statistics
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int MyTickets { get; set; }
    public int ResolvedTickets { get; set; }
    
    // User Information
    public List<string> UserRoles { get; set; } = new();
    
    // Recent Activities
    public List<RecentRoleMapping> RecentRoleMappings { get; set; } = new();
}

public class RecentRoleMapping
{
    public string CustomerServiceRole { get; set; } = string.Empty;
    public string AspNetRoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}