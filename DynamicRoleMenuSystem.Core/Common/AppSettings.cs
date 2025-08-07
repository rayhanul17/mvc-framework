namespace DynamicRoleMenuSystem.Core.Common;

public class AppSettings
{
    public BrandingSettings Branding { get; set; } = new();
    public ThemeSettings Theme { get; set; } = new();
    public LayoutSettings Layout { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public FeatureSettings Features { get; set; } = new();
}

public class BrandingSettings
{
    public string OrganizationName { get; set; } = "Dynamic Role Menu System";
    public string ShortName { get; set; } = "DRMS";
    public string Slogan { get; set; } = "Empowering Dynamic Access Control";
    public string Description { get; set; } = "A comprehensive role-based menu management system";
    public string LogoPath { get; set; } = "/images/logo.png";
    public string FaviconPath { get; set; } = "/favicon.ico";
    public string Version { get; set; } = "2.0";
    public ContactInfo Contact { get; set; } = new();
}

public class ContactInfo
{
    public string Email { get; set; } = "admin@drms.com";
    public string Phone { get; set; } = "+1-234-567-8900";
    public string Address { get; set; } = "123 Tech Street, Digital City";
    public string Website { get; set; } = "https://drms.example.com";
    public SocialMediaLinks SocialMedia { get; set; } = new();
}

public class SocialMediaLinks
{
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? LinkedIn { get; set; }
    public string? GitHub { get; set; }
    public string? Instagram { get; set; }
}

public class ThemeSettings
{
    public ColorScheme Colors { get; set; } = new();
    public string DefaultTheme { get; set; } = "light"; // light, dark, auto
    public bool AllowUserThemeSelection { get; set; } = true;
    public TypographySettings Typography { get; set; } = new();
}

public class ColorScheme
{
    public string Primary { get; set; } = "#007bff";
    public string Secondary { get; set; } = "#6c757d";
    public string Success { get; set; } = "#28a745";
    public string Danger { get; set; } = "#dc3545";
    public string Warning { get; set; } = "#ffc107";
    public string Info { get; set; } = "#17a2b8";
    public string Light { get; set; } = "#f8f9fa";
    public string Dark { get; set; } = "#343a40";
    public string Background { get; set; } = "#ffffff";
    public string Surface { get; set; } = "#f8f9fa";
    public string OnPrimary { get; set; } = "#ffffff";
    public string OnSecondary { get; set; } = "#ffffff";
    public string OnBackground { get; set; } = "#212529";
    public string OnSurface { get; set; } = "#212529";
}

public class TypographySettings
{
    public string FontFamily { get; set; } = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
    public string HeadingFontFamily { get; set; } = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
    public string MonospaceFontFamily { get; set; } = "'Courier New', Courier, monospace";
    public string BaseFontSize { get; set; } = "1rem";
    public string LineHeight { get; set; } = "1.5";
}

public class LayoutSettings
{
    public SidebarSettings Sidebar { get; set; } = new();
    public HeaderSettings Header { get; set; } = new();
    public FooterSettings Footer { get; set; } = new();
    public ContentSettings Content { get; set; } = new();
}

public class SidebarSettings
{
    public bool IsCollapsible { get; set; } = true;
    public bool DefaultCollapsed { get; set; } = false;
    public string Width { get; set; } = "250px";
    public string CollapsedWidth { get; set; } = "80px";
    public bool ShowIcons { get; set; } = true;
    public bool ShowTooltips { get; set; } = true;
}

public class HeaderSettings
{
    public bool ShowBreadcrumbs { get; set; } = true;
    public bool ShowUserProfile { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public bool ShowThemeToggle { get; set; } = true;
    public bool ShowSearch { get; set; } = true;
    public string Height { get; set; } = "60px";
}

public class FooterSettings
{
    public bool Show { get; set; } = true;
    public string CopyrightText { get; set; } = "© {year} {organizationName}. All rights reserved.";
    public bool ShowVersion { get; set; } = true;
    public bool ShowLinks { get; set; } = true;
    public List<FooterLink> Links { get; set; } = new()
    {
        new FooterLink { Text = "Privacy Policy", Url = "/privacy" },
        new FooterLink { Text = "Terms of Service", Url = "/terms" },
        new FooterLink { Text = "Support", Url = "/support" }
    };
}

public class FooterLink
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool OpenInNewTab { get; set; } = false;
}

public class ContentSettings
{
    public string MaxWidth { get; set; } = "1200px";
    public string Padding { get; set; } = "20px";
    public bool ShowPageTitle { get; set; } = true;
    public bool ShowBreadcrumbs { get; set; } = true;
}

public class SecuritySettings
{
    public bool RequireHttps { get; set; } = true;
    public bool EnableTwoFactor { get; set; } = false;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int PasswordExpiryDays { get; set; } = 90;
    public bool RequirePasswordChange { get; set; } = false;
}

public class FeatureSettings
{
    public bool EnableUserRegistration { get; set; } = true;
    public bool EnablePasswordReset { get; set; } = true;
    public bool EnableEmailConfirmation { get; set; } = false;
    public bool EnableAuditLog { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public bool EnableCompression { get; set; } = true;
    public bool EnableRateLimiting { get; set; } = false;
    public DashboardSettings Dashboard { get; set; } = new();
}

public class DashboardSettings
{
    public bool ShowWelcomeMessage { get; set; } = true;
    public bool ShowStatistics { get; set; } = true;
    public bool ShowRecentActivity { get; set; } = true;
    public bool ShowQuickActions { get; set; } = true;
    public int RecentActivityCount { get; set; } = 10;
}