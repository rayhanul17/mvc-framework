namespace Nexora.Web.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    public string SiteName { get; set; } = "Nexora Framework";
    public string SiteSlogan { get; set; } = "Dynamic Role-Based System";
    public string WelcomeMessage { get; set; } = "Welcome to Admin Dashboard";
    
    // User Statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public int TotalRoles { get; set; }
    
    // Blog Statistics
    public int TotalBlogPosts { get; set; }
    public int PublishedBlogPosts { get; set; }
    public int DraftBlogPosts { get; set; }
    
    // System Health
    public string DatabaseStatus { get; set; } = "Unknown";
    public DateTime LastBackupDate { get; set; }
    public SystemMetrics SystemMetrics { get; set; } = new();
    
    // Recent Activities
    public List<RecentUserActivity> RecentUsers { get; set; } = new();
    public List<RecentSystemEvent> RecentEvents { get; set; } = new();
}

public class RecentUserActivity
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string ActivityType { get; set; } = "Registration";
}

public class RecentSystemEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "Info";
    public string UserName { get; set; } = string.Empty;
}

public class SystemMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveSessions { get; set; }
    public int RequestsPerMinute { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class QuickStats
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "primary";
    public string Change { get; set; } = string.Empty;
    public bool IsIncrease { get; set; }
}