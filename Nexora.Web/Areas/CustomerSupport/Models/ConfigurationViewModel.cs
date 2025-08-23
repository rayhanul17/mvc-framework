using Nexora.Core.Entities;

namespace Nexora.Web.Areas.CustomerSupport.Models;

public class PrioritySLA
{
    public string Priority { get; set; } = "";
    public int ResponseTimeHours { get; set; }
    public int ResolutionTimeHours { get; set; }
}

public class ConfigurationViewModel
{
    // Categories
    public List<string> Categories { get; set; } = new();
    public string NewCategory { get; set; } = "";

    // Support Agents
    public List<ApplicationUser> SupportAgents { get; set; } = new();

    // Priority & SLA
    public List<PrioritySLA> PrioritySLAs { get; set; } = new();

    // Auto-Assignment
    public bool AutoAssignmentEnabled { get; set; }
    public string DefaultAssignmentMethod { get; set; } = "RoundRobin";

    // Notifications
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }

    // Business Hours
    public string BusinessHoursStart { get; set; } = "09:00";
    public string BusinessHoursEnd { get; set; } = "18:00";
    public string WorkingDays { get; set; } = "";

    // Statistics
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResponseTime { get; set; }
    public double AverageResolutionTime { get; set; }
}