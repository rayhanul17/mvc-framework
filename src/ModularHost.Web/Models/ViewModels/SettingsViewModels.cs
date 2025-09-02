using System.ComponentModel.DataAnnotations;

namespace MRCMS.Models.ViewModels
{
    public class GeneralSettingsViewModel
    {
        [Required]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "ModularHost";

        [Display(Name = "Site Short Name")]
        [MaxLength(20)]
        public string SiteShortName { get; set; } = "MRCMS";

        [Display(Name = "Site Tagline")]
        [MaxLength(100)]
        public string SiteTagline { get; set; } = "Modern Blog Platform";

        [Display(Name = "Site Description")]
        [DataType(DataType.MultilineText)]
        public string SiteDescription { get; set; } = "";

        [Display(Name = "Site Keywords")]
        public string SiteKeywords { get; set; } = "blog, cms, content management";

        [Required]
        [Url]
        [Display(Name = "Site URL")]
        public string SiteUrl { get; set; } = "https://localhost";

        [Display(Name = "Site Logo URL")]
        public string SiteLogo { get; set; } = "/img/logo.png";

        [Display(Name = "Site Favicon URL")]
        public string SiteFavicon { get; set; } = "/favicon.ico";

        [Display(Name = "Site Footer Logo")]
        public string FooterLogo { get; set; } = "/img/footer-logo.png";

        [Display(Name = "Copyright Text")]
        public string CopyrightText { get; set; } = "© 2024 ModularHost. All rights reserved.";

        [Display(Name = "Google Analytics ID")]
        public string GoogleAnalyticsId { get; set; } = "";

        [Display(Name = "Facebook URL")]
        [Url]
        public string FacebookUrl { get; set; } = "";

        [Display(Name = "Twitter URL")]
        [Url]
        public string TwitterUrl { get; set; } = "";

        [Display(Name = "LinkedIn URL")]
        [Url]
        public string LinkedInUrl { get; set; } = "";

        [Display(Name = "Instagram URL")]
        [Url]
        public string InstagramUrl { get; set; } = "";

        [Display(Name = "YouTube URL")]
        [Url]
        public string YouTubeUrl { get; set; } = "";

        [Required]
        [Display(Name = "Time Zone")]
        public string SiteTimezone { get; set; } = "UTC";

        [Required]
        [Display(Name = "Default Language")]
        public string DefaultLanguage { get; set; } = "en-US";

        [Required]
        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "MM/dd/yyyy";

        [Required]
        [Display(Name = "Time Format")]
        public string TimeFormat { get; set; } = "hh:mm tt";

        [Range(5, 100)]
        [Display(Name = "Page Size")]
        public int PageSize { get; set; } = 10;

        [Display(Name = "Enable Registration")]
        public bool EnableRegistration { get; set; } = true;

        [Display(Name = "Require Email Confirmation")]
        public bool RequireEmailConfirmation { get; set; } = false;

        [Display(Name = "Enable Comments")]
        public bool EnableComments { get; set; } = true;

        [Display(Name = "Moderate Comments")]
        public bool ModerateComments { get; set; } = true;

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; } = false;

        [Display(Name = "Maintenance Message")]
        [DataType(DataType.MultilineText)]
        public string MaintenanceMessage { get; set; } = "Site is under maintenance";

        [Display(Name = "Contact Email")]
        [EmailAddress]
        public string ContactEmail { get; set; } = "";

        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = "";

        [Display(Name = "Contact Address")]
        [DataType(DataType.MultilineText)]
        public string ContactAddress { get; set; } = "";
    }

    public class EmailSettingsViewModel
    {
        [Required]
        [Display(Name = "SMTP Host")]
        public string SmtpHost { get; set; } = "smtp.gmail.com";

        [Required]
        [Range(1, 65535)]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string SmtpPassword { get; set; } = "";

        [Display(Name = "Enable SSL/TLS")]
        public bool EnableSsl { get; set; } = true;

        [Required]
        [EmailAddress]
        [Display(Name = "From Email")]
        public string FromEmail { get; set; } = "noreply@example.com";

        [Required]
        [Display(Name = "From Name")]
        public string FromName { get; set; } = "ModularHost";

        [EmailAddress]
        [Display(Name = "Reply-To Email")]
        public string? ReplyToEmail { get; set; }

        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;

        [Display(Name = "Use SSL")]
        public bool UseSsl { get; set; } = true;

        [EmailAddress]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; } = "";

        [EmailAddress]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; } = "";

        [Display(Name = "Send Welcome Email")]
        public bool SendWelcomeEmail { get; set; } = true;

        [Display(Name = "Send Admin Notifications")]
        public bool SendAdminNotifications { get; set; } = true;
    }

    public class SecuritySettingsViewModel
    {
        [Range(6, 32)]
        [Display(Name = "Minimum Password Length")]
        public int PasswordMinLength { get; set; } = 8;

        [Display(Name = "Require Digit")]
        public bool PasswordRequireDigit { get; set; } = true;

        [Display(Name = "Require Lowercase")]
        public bool PasswordRequireLowercase { get; set; } = true;

        [Display(Name = "Require Uppercase")]
        public bool PasswordRequireUppercase { get; set; } = true;

        [Display(Name = "Require Non-Alphanumeric")]
        public bool PasswordRequireNonAlphanumeric { get; set; } = true;

        [Display(Name = "Enable Account Lockout")]
        public bool LockoutEnabled { get; set; } = true;

        [Range(3, 10)]
        [Display(Name = "Max Failed Access Attempts")]
        public int MaxFailedAccessAttempts { get; set; } = 5;

        [Range(5, 1440)]
        [Display(Name = "Lockout Duration (minutes)")]
        public int LockoutDurationMinutes { get; set; } = 15;

        [Display(Name = "Enable Two-Factor Authentication")]
        public bool EnableTwoFactorAuthentication { get; set; } = false;

        [Range(5, 1440)]
        [Display(Name = "Session Timeout (minutes)")]
        public int SessionTimeoutMinutes { get; set; } = 30;

        [Display(Name = "Require HTTPS")]
        public bool RequireHttps { get; set; } = true;

        [Display(Name = "Enable Audit Log")]
        public bool EnableAuditLog { get; set; } = true;

        [Display(Name = "Enable IP Whitelisting")]
        public bool EnableIpWhitelisting { get; set; } = false;

        [Display(Name = "IP Whitelist")]
        public string? IpWhitelist { get; set; }
    }

    public class AppearanceSettingsViewModel
    {
        [Display(Name = "Theme")]
        public string Theme { get; set; } = "default";

        [Display(Name = "Enable Dark Mode")]
        public bool EnableDarkMode { get; set; } = true;

        [Display(Name = "Primary Color")]
        public string PrimaryColor { get; set; } = "#3b82f6";

        [Display(Name = "Secondary Color")]
        public string SecondaryColor { get; set; } = "#64748b";

        [Display(Name = "Logo URL")]
        public string? LogoUrl { get; set; }

        [Display(Name = "Favicon URL")]
        public string? FaviconUrl { get; set; }

        [Display(Name = "Custom CSS")]
        public string? CustomCss { get; set; }

        [Display(Name = "Custom JavaScript")]
        public string? CustomJavaScript { get; set; }

        [Display(Name = "Footer Text")]
        public string FooterText { get; set; } = "© 2024 ModularHost. All rights reserved.";

        [Display(Name = "Show Footer")]
        public bool ShowFooter { get; set; } = true;
    }

    public class NotificationSettingsViewModel
    {
        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;

        [Display(Name = "Enable Push Notifications")]
        public bool EnablePushNotifications { get; set; } = false;

        [Display(Name = "Enable In-App Notifications")]
        public bool EnableInAppNotifications { get; set; } = true;

        [Display(Name = "Notification Retention Days")]
        [Range(1, 365)]
        public int NotificationRetentionDays { get; set; } = 30;

        [Display(Name = "Send Welcome Email")]
        public bool SendWelcomeEmail { get; set; } = true;

        [Display(Name = "Send Password Reset Email")]
        public bool SendPasswordResetEmail { get; set; } = true;

        [Display(Name = "Send Account Locked Email")]
        public bool SendAccountLockedEmail { get; set; } = true;

        [Display(Name = "Admin Email for Alerts")]
        [EmailAddress]
        public string? AdminAlertEmail { get; set; }

        [Display(Name = "Daily Summary Email")]
        public bool SendDailySummary { get; set; } = false;

        [Display(Name = "Weekly Report Email")]
        public bool SendWeeklyReport { get; set; } = false;
    }

    public class PrivacySettingsViewModel
    {
        [Display(Name = "Enable Cookie Consent")]
        public bool EnableCookieConsent { get; set; } = true;

        [Display(Name = "Cookie Consent Message")]
        public string CookieConsentMessage { get; set; } = "This website uses cookies to ensure you get the best experience.";

        [Display(Name = "Enable GDPR Compliance")]
        public bool EnableGdprCompliance { get; set; } = true;

        [Display(Name = "Privacy Policy URL")]
        [Url]
        public string? PrivacyPolicyUrl { get; set; }

        [Display(Name = "Terms of Service URL")]
        [Url]
        public string? TermsOfServiceUrl { get; set; }

        [Display(Name = "Data Retention Days")]
        [Range(30, 3650)]
        public int DataRetentionDays { get; set; } = 365;

        [Display(Name = "Allow User Data Export")]
        public bool AllowUserDataExport { get; set; } = true;

        [Display(Name = "Allow User Data Deletion")]
        public bool AllowUserDataDeletion { get; set; } = true;

        [Display(Name = "Anonymous Usage Statistics")]
        public bool CollectAnonymousStatistics { get; set; } = true;

        [Display(Name = "Third-Party Analytics")]
        public bool EnableThirdPartyAnalytics { get; set; } = false;

        [Display(Name = "Analytics Tracking ID")]
        public string? AnalyticsTrackingId { get; set; }
    }

    public class AdvancedSettingsViewModel
    {
        [Display(Name = "Enable Caching")]
        public bool EnableCaching { get; set; } = true;

        [Display(Name = "Cache Duration (minutes)")]
        [Range(1, 1440)]
        public int CacheDurationMinutes { get; set; } = 60;

        [Display(Name = "Enable CDN")]
        public bool EnableCdn { get; set; } = false;

        [Display(Name = "CDN URL")]
        [Url]
        public string? CdnUrl { get; set; }

        [Display(Name = "Enable API")]
        public bool EnableApi { get; set; } = true;

        [Display(Name = "API Rate Limit (per minute)")]
        [Range(10, 1000)]
        public int ApiRateLimit { get; set; } = 60;

        [Display(Name = "Enable WebSockets")]
        public bool EnableWebSockets { get; set; } = true;

        [Display(Name = "Enable Background Jobs")]
        public bool EnableBackgroundJobs { get; set; } = true;

        [Display(Name = "Job Retention Days")]
        [Range(1, 90)]
        public int JobRetentionDays { get; set; } = 7;

        [Display(Name = "Enable Development Mode")]
        public bool EnableDevelopmentMode { get; set; } = false;

        [Display(Name = "Enable Debug Logging")]
        public bool EnableDebugLogging { get; set; } = false;

        [Display(Name = "Log Level")]
        public string LogLevel { get; set; } = "Information";

        [Display(Name = "Max Upload Size (MB)")]
        [Range(1, 100)]
        public int MaxUploadSizeMb { get; set; } = 10;

        [Display(Name = "Allowed File Extensions")]
        public string AllowedFileExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx";
    }
}