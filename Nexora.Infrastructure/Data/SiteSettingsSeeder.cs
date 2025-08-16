using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data;

public static class SiteSettingsSeeder
{
    public static List<SiteSetting> GetDefaultSettings()
    {
        return new List<SiteSetting>
        {
            // Branding Settings
            new SiteSetting
            {
                Key = "OrganizationName",
                Value = "Nexora",
                Description = "Full organization/company name",
                Category = SettingCategory.Branding,
                Type = SettingType.Text,
                IsRequired = true,
                IsSystemSetting = true,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "ShortName",
                Value = "NXR",
                Description = "Short name or abbreviation (3-5 characters)",
                Category = SettingCategory.Branding,
                Type = SettingType.Text,
                IsRequired = true,
                IsSystemSetting = true,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "Slogan",
                Value = "Empowering Your Digital Future",
                Description = "Company slogan or tagline",
                Category = SettingCategory.Branding,
                Type = SettingType.Text,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "SmallLogo",
                Value = "/images/logo-small.png",
                Description = "Small logo for mobile/collapsed sidebar (40x40px recommended)",
                Category = SettingCategory.Branding,
                Type = SettingType.File,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 4,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "LargeLogo",
                Value = "/images/logo-large.png",
                Description = "Large logo for desktop (200x50px recommended)",
                Category = SettingCategory.Branding,
                Type = SettingType.File,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 5,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "FaviconPath",
                Value = "/favicon.ico",
                Description = "Browser favicon (32x32px .ico file)",
                Category = SettingCategory.Branding,
                Type = SettingType.File,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 6,
                CreatedAt = DateTime.UtcNow
            },
            
            // Theme Settings
            new SiteSetting
            {
                Key = "PrimaryColor",
                Value = "#0d6efd",
                Description = "Primary brand color",
                Category = SettingCategory.Theme,
                Type = SettingType.Color,
                IsRequired = true,
                IsSystemSetting = false,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "SecondaryColor",
                Value = "#6c757d",
                Description = "Secondary brand color",
                Category = SettingCategory.Theme,
                Type = SettingType.Color,
                IsRequired = true,
                IsSystemSetting = false,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "DarkModeDefault",
                Value = "false",
                Description = "Enable dark mode by default",
                Category = SettingCategory.Theme,
                Type = SettingType.Boolean,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            },
            
            // Layout Settings
            new SiteSetting
            {
                Key = "FooterHtml",
                Value = @"<div class='text-center'>
    <p class='mb-2'>© 2024 <strong>Nexora</strong>. All rights reserved.</p>
    <div class='footer-links'>
        <a href='/privacy' class='text-muted me-3'>Privacy Policy</a>
        <a href='/terms' class='text-muted me-3'>Terms of Service</a>
        <a href='/contact' class='text-muted'>Contact Us</a>
    </div>
</div>",
                Description = "Footer HTML content",
                Category = SettingCategory.Layout,
                Type = SettingType.TextArea,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "StickyFooter",
                Value = "true",
                Description = "Make footer stick to bottom of page",
                Category = SettingCategory.Layout,
                Type = SettingType.Boolean,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "SidebarCollapsedDefault",
                Value = "false",
                Description = "Start with sidebar collapsed",
                Category = SettingCategory.Layout,
                Type = SettingType.Boolean,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            },
            
            // Contact Settings
            new SiteSetting
            {
                Key = "ContactEmail",
                Value = "contact@nexora.com",
                Description = "Main contact email address",
                Category = SettingCategory.Contact,
                Type = SettingType.Email,
                IsRequired = true,
                IsSystemSetting = false,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "SupportEmail",
                Value = "support@nexora.com",
                Description = "Support team email address",
                Category = SettingCategory.Contact,
                Type = SettingType.Email,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "PhoneNumber",
                Value = "+1 (555) 123-4567",
                Description = "Main contact phone number",
                Category = SettingCategory.Contact,
                Type = SettingType.Text,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "Address",
                Value = "123 Business St, Suite 100, City, State 12345",
                Description = "Physical address",
                Category = SettingCategory.Contact,
                Type = SettingType.TextArea,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 4,
                CreatedAt = DateTime.UtcNow
            },
            
            // Social Settings
            new SiteSetting
            {
                Key = "FacebookUrl",
                Value = "",
                Description = "Facebook page URL",
                Category = SettingCategory.Social,
                Type = SettingType.Url,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "TwitterUrl",
                Value = "",
                Description = "Twitter/X profile URL",
                Category = SettingCategory.Social,
                Type = SettingType.Url,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "LinkedInUrl",
                Value = "",
                Description = "LinkedIn company page URL",
                Category = SettingCategory.Social,
                Type = SettingType.Url,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "GitHubUrl",
                Value = "",
                Description = "GitHub organization URL",
                Category = SettingCategory.Social,
                Type = SettingType.Url,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 4,
                CreatedAt = DateTime.UtcNow
            },
            
            // Features
            new SiteSetting
            {
                Key = "EnableRegistration",
                Value = "true",
                Description = "Allow new user registrations",
                Category = SettingCategory.Features,
                Type = SettingType.Boolean,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "MaintenanceMode",
                Value = "false",
                Description = "Enable maintenance mode",
                Category = SettingCategory.Features,
                Type = SettingType.Boolean,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 2,
                CreatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Key = "MaintenanceMessage",
                Value = "We are currently performing scheduled maintenance. Please check back soon.",
                Description = "Message to display during maintenance",
                Category = SettingCategory.Features,
                Type = SettingType.TextArea,
                IsRequired = false,
                IsSystemSetting = false,
                Order = 3,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}