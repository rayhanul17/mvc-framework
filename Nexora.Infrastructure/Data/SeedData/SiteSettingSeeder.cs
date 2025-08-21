using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class SiteSettingSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.SiteSettings.AnyAsync())
        {
            var settings = GetDefaultSettings();
            await context.SiteSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }
    
    private static List<SiteSetting> GetDefaultSettings()
    {
        return new List<SiteSetting>
        {
            new SiteSetting { Key = "SiteName", Value = "Nexora MVC Framework", Category = SettingCategory.Branding, Type = SettingType.Text },
            new SiteSetting { Key = "SiteDescription", Value = "A modern MVC framework for building scalable web applications", Category = SettingCategory.Branding, Type = SettingType.TextArea },
            new SiteSetting { Key = "SiteKeywords", Value = "mvc, framework, web development, asp.net core", Category = SettingCategory.Advanced, Type = SettingType.TextArea },
            new SiteSetting { Key = "SiteAuthor", Value = "Nexora Team", Category = SettingCategory.Branding, Type = SettingType.Text },
            new SiteSetting { Key = "ContactEmail", Value = "contact@nexora.com", Category = SettingCategory.Contact, Type = SettingType.Email },
            new SiteSetting { Key = "ContactPhone", Value = "+1 234 567 8900", Category = SettingCategory.Contact, Type = SettingType.Text },
            new SiteSetting { Key = "ContactAddress", Value = "123 Tech Street, Silicon Valley, CA 94025", Category = SettingCategory.Contact, Type = SettingType.TextArea },
            new SiteSetting { Key = "FacebookUrl", Value = "https://facebook.com/nexora", Category = SettingCategory.Social, Type = SettingType.Url },
            new SiteSetting { Key = "TwitterUrl", Value = "https://twitter.com/nexora", Category = SettingCategory.Social, Type = SettingType.Url },
            new SiteSetting { Key = "LinkedInUrl", Value = "https://linkedin.com/company/nexora", Category = SettingCategory.Social, Type = SettingType.Url },
            new SiteSetting { Key = "GitHubUrl", Value = "https://github.com/nexora", Category = SettingCategory.Social, Type = SettingType.Url },
            new SiteSetting { Key = "EnableRegistration", Value = "true", Category = SettingCategory.Features, Type = SettingType.Boolean },
            new SiteSetting { Key = "RequireEmailConfirmation", Value = "false", Category = SettingCategory.Features, Type = SettingType.Boolean },
            new SiteSetting { Key = "MaxUploadSize", Value = "10485760", Category = SettingCategory.Advanced, Type = SettingType.Number }, // 10MB
            new SiteSetting { Key = "AllowedFileExtensions", Value = ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx,.zip", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "DefaultTimeZone", Value = "UTC", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "DateFormat", Value = "MM/dd/yyyy", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "TimeFormat", Value = "hh:mm tt", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "BlogPostsPerPage", Value = "10", Category = SettingCategory.Features, Type = SettingType.Number },
            new SiteSetting { Key = "EnableComments", Value = "true", Category = SettingCategory.Features, Type = SettingType.Boolean },
            new SiteSetting { Key = "ModerateComments", Value = "false", Category = SettingCategory.Features, Type = SettingType.Boolean },
            new SiteSetting { Key = "GoogleAnalyticsId", Value = "", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "GoogleMapsApiKey", Value = "", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "SmtpHost", Value = "smtp.gmail.com", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "SmtpPort", Value = "587", Category = SettingCategory.Advanced, Type = SettingType.Number },
            new SiteSetting { Key = "SmtpUsername", Value = "", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "SmtpPassword", Value = "", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "SmtpEnableSsl", Value = "true", Category = SettingCategory.Advanced, Type = SettingType.Boolean },
            new SiteSetting { Key = "EmailFromAddress", Value = "noreply@nexora.com", Category = SettingCategory.Advanced, Type = SettingType.Email },
            new SiteSetting { Key = "EmailFromName", Value = "Nexora", Category = SettingCategory.Advanced, Type = SettingType.Text },
            new SiteSetting { Key = "MaintenanceMode", Value = "false", Category = SettingCategory.Features, Type = SettingType.Boolean },
            new SiteSetting { Key = "MaintenanceMessage", Value = "We are currently performing maintenance. Please check back later.", Category = SettingCategory.Features, Type = SettingType.TextArea },
            new SiteSetting { Key = "CookieConsentEnabled", Value = "true", Category = SettingCategory.Security, Type = SettingType.Boolean },
            new SiteSetting { Key = "CookieConsentMessage", Value = "This website uses cookies to ensure you get the best experience on our website.", Category = SettingCategory.Security, Type = SettingType.TextArea },
            new SiteSetting { Key = "PrivacyPolicyUrl", Value = "/privacy", Category = SettingCategory.Security, Type = SettingType.Url },
            new SiteSetting { Key = "TermsOfServiceUrl", Value = "/terms", Category = SettingCategory.Security, Type = SettingType.Url }
        };
    }
}