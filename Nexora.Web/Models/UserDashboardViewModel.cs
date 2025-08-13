namespace Nexora.Web.Models;

public class UserDashboardViewModel
{
    public string SiteName { get; set; } = "Nexora Framework";
    public string SiteSlogan { get; set; } = "Dynamic Role-Based System";
    public string WelcomeMessage { get; set; } = "Welcome to your dashboard";
    
    // User Information
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    public List<string> UserRoles { get; set; } = new();
    
    // User Activity
    public DateTime LastLogin { get; set; }
    public int ProfileCompletion { get; set; } = 75; // Placeholder
    
    // Quick Stats
    public List<UserQuickStat> QuickStats { get; set; } = new();
    
    // Recent Activities
    public List<UserActivity> RecentActivities { get; set; } = new();
}

public class UserQuickStat
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "primary";
    public string Description { get; set; } = string.Empty;
}

public class UserActivity
{
    public string Activity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "fas fa-info";
    public string Color { get; set; } = "primary";
}