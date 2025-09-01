using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MRCMS.Core.Models.Entities;

namespace MRCMS.ViewModels
{
    // Admin Dashboard ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalMenus { get; set; }
        public int TotalPermissions { get; set; }
        public List<UserSummary> RecentUsers { get; set; } = new();
        public List<AuditLogEntry> RecentAuditLogs { get; set; } = new();
        public SystemStats SystemStats { get; set; } = new();
    }

    public class SystemStats
    {
        public long DatabaseSize { get; set; }
        public long CacheSize { get; set; }
        public long LogSize { get; set; }
        public string FormattedDatabaseSize => FormatBytes(DatabaseSize);
        public string FormattedCacheSize => FormatBytes(CacheSize);
        public string FormattedLogSize => FormatBytes(LogSize);

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }

    public class UserSummary
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
    }

    // Settings ViewModels
    public class GeneralSettingsViewModel
    {
        [Required]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "";

        [Display(Name = "Site Description")]
        public string SiteDescription { get; set; } = "";

        [Required]
        [Display(Name = "Site URL")]
        [Url]
        public string SiteUrl { get; set; } = "";

        [Display(Name = "Time Zone")]
        public string SiteTimezone { get; set; } = "UTC";

        [Display(Name = "Default Language")]
        public string DefaultLanguage { get; set; } = "en-US";

        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "MM/dd/yyyy";

        [Display(Name = "Time Format")]
        public string TimeFormat { get; set; } = "hh:mm tt";

        [Display(Name = "Items Per Page")]
        [Range(5, 100)]
        public int PageSize { get; set; } = 10;

        [Display(Name = "Enable User Registration")]
        public bool EnableRegistration { get; set; }

        [Display(Name = "Require Email Confirmation")]
        public bool RequireEmailConfirmation { get; set; }

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; }

        [Display(Name = "Maintenance Message")]
        public string MaintenanceMessage { get; set; } = "";
    }

    public class EmailSettingsViewModel
    {
        [Required]
        [Display(Name = "SMTP Host")]
        public string SmtpHost { get; set; } = "";

        [Required]
        [Display(Name = "SMTP Port")]
        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; } = "";

        [Display(Name = "SMTP Password")]
        [DataType(DataType.Password)]
        public string SmtpPassword { get; set; } = "";

        [Display(Name = "Enable SSL/TLS")]
        public bool EnableSsl { get; set; } = true;

        [Required]
        [Display(Name = "From Email")]
        [EmailAddress]
        public string FromEmail { get; set; } = "";

        [Display(Name = "From Name")]
        public string FromName { get; set; } = "";

        [Display(Name = "Reply-To Email")]
        [EmailAddress]
        public string? ReplyToEmail { get; set; }

        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;

        [Display(Name = "Use SSL")]
        public bool UseSsl { get; set; } = true;

        [Display(Name = "Admin Email")]
        [EmailAddress]
        public string AdminEmail { get; set; } = "";

        [Display(Name = "Support Email")]
        [EmailAddress]
        public string SupportEmail { get; set; } = "";

        [Display(Name = "Send Welcome Email")]
        public bool SendWelcomeEmail { get; set; } = true;

        [Display(Name = "Send Admin Notifications")]
        public bool SendAdminNotifications { get; set; } = true;
    }

    public class SecuritySettingsViewModel
    {
        [Display(Name = "Minimum Password Length")]
        [Range(6, 20)]
        public int PasswordMinLength { get; set; } = 8;

        [Display(Name = "Require Digit")]
        public bool PasswordRequireDigit { get; set; } = true;

        [Display(Name = "Require Lowercase")]
        public bool PasswordRequireLowercase { get; set; } = true;

        [Display(Name = "Require Uppercase")]
        public bool PasswordRequireUppercase { get; set; } = true;

        [Display(Name = "Require Special Character")]
        public bool PasswordRequireNonAlphanumeric { get; set; } = true;

        [Display(Name = "Enable Account Lockout")]
        public bool LockoutEnabled { get; set; } = true;

        [Display(Name = "Max Failed Access Attempts")]
        [Range(3, 10)]
        public int MaxFailedAccessAttempts { get; set; } = 5;

        [Display(Name = "Lockout Duration (minutes)")]
        [Range(5, 60)]
        public int LockoutDurationMinutes { get; set; } = 15;

        [Display(Name = "Enable Two-Factor Authentication")]
        public bool EnableTwoFactorAuthentication { get; set; }

        [Display(Name = "Session Timeout (minutes)")]
        [Range(5, 1440)]
        public int SessionTimeoutMinutes { get; set; } = 30;

        [Display(Name = "Require HTTPS")]
        public bool RequireHttps { get; set; } = true;

        [Display(Name = "Enable Audit Logging")]
        public bool EnableAuditLog { get; set; } = true;

        [Display(Name = "Enable IP Whitelisting")]
        public bool EnableIpWhitelisting { get; set; }

        [Display(Name = "IP Whitelist (comma-separated)")]
        public string? IpWhitelist { get; set; }
    }

    // Role ViewModels
    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
    }

    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role Name")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class EditRoleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    public class ManagePermissionsViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public List<RolePermission> Permissions { get; set; } = new();
        public List<Menu> AvailableMenus { get; set; } = new();
    }

    public class ManageRoleUsersViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public List<UserSummary> UsersInRole { get; set; } = new();
        public List<UserSummary> UsersNotInRole { get; set; } = new();
    }

    // Report ViewModels
    public class UserActivityReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersInPeriod { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public List<DailyStatistic> DailyRegistrations { get; set; } = new();
        public List<UserActivitySummary> TopActiveUsers { get; set; } = new();
        public double UserGrowthRate { get; set; }
        public int TotalLogins { get; set; }
        public int UniqueUserLogins { get; set; }
        public int FailedLoginAttempts { get; set; }
    }

    public class BlogStatsReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPosts { get; set; }
        public int PublishedPosts { get; set; }
        public int DraftPosts { get; set; }
        public int TotalViews { get; set; }
        public int TotalComments { get; set; }
        public int PostsInPeriod { get; set; }
        public List<PostSummary> MostViewedPosts { get; set; } = new();
        public List<PostSummary> MostCommentedPosts { get; set; } = new();
        public List<AuthorStatistic> TopAuthors { get; set; } = new();
        public Dictionary<string, int> PostsByCategory { get; set; } = new();
        public Dictionary<string, int> PostsByTag { get; set; } = new();
        public List<DailyStatistic> DailyPostStats { get; set; } = new();
        public double AverageViewsPerPost { get; set; }
        public double AverageCommentsPerPost { get; set; }
    }

    public class DailyStatistic
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class UserActivitySummary
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public int LoginCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class PostSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class AuthorStatistic
    {
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public int PostCount { get; set; }
        public int TotalViews { get; set; }
    }

    public class SystemLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Source { get; set; }
        public string? Exception { get; set; }
    }

    // Audit Log ViewModels
    public class AuditLogEntry
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = "";
        public string Action { get; set; } = "";
        public string? Details { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AuditLogViewModel
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = "";
        public Guid EntityId { get; set; }
        public string Action { get; set; } = "";
        public int VersionNumber { get; set; }
        public string UserName { get; set; } = "";
        public Guid? UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? ChangedProperties { get; set; }
        public bool HasChanges { get; set; }
    }

    public class AuditDiffViewModel
    {
        public AuditVersionInfo? CurrentVersion { get; set; }
        public AuditVersionInfo? PreviousVersion { get; set; }
    }

    public class AuditVersionInfo
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = "";
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public List<string> ChangedProperties { get; set; } = new();
    }

    public class AuditHistoryItem
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = "";
        public string? ChangedProperties { get; set; }
    }

    // Login Statistics
    public class LoginStatistics
    {
        public int TotalLogins { get; set; }
        public int UniqueUsers { get; set; }
        public int FailedAttempts { get; set; }
    }

    // User Login Count
    public class UserLoginCount
    {
        public Guid UserId { get; set; }
        public int Count { get; set; }
        public DateTime LastLogin { get; set; }
    }
}