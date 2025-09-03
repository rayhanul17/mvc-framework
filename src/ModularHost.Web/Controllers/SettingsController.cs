using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Services.Interfaces;
using MRCMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using MRCMS.Services;

namespace MRCMS.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogger _auditLogger;
        private readonly IEmailService _emailService;

        public SettingsController(
            AppDbContext context,
            IConfiguration configuration,
            IAuditLogger auditLogger,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _auditLogger = auditLogger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> General()
        {
            var settings = await GetSettingsByCategoryAsync("General");
            var viewModel = new GeneralSettingsViewModel
            {
                SiteName = GetSettingValue(settings, "Site.Name", "ModularHost"),
                SiteDescription = GetSettingValue(settings, "Site.Description", "A modular web application"),
                SiteUrl = GetSettingValue(settings, "Site.Url", "https://localhost"),
                SiteTimezone = GetSettingValue(settings, "Site.Timezone", "UTC"),
                DefaultLanguage = GetSettingValue(settings, "Site.Language", "en-US"),
                DateFormat = GetSettingValue(settings, "Site.DateFormat", "MM/dd/yyyy"),
                TimeFormat = GetSettingValue(settings, "Site.TimeFormat", "hh:mm tt"),
                PageSize = int.Parse(GetSettingValue(settings, "Site.PageSize", "10")),
                EnableRegistration = bool.Parse(GetSettingValue(settings, "Site.EnableRegistration", "true")),
                RequireEmailConfirmation = bool.Parse(GetSettingValue(settings, "Site.RequireEmailConfirmation", "false")),
                MaintenanceMode = bool.Parse(GetSettingValue(settings, "Site.MaintenanceMode", "false")),
                MaintenanceMessage = GetSettingValue(settings, "Site.MaintenanceMessage", "Site is under maintenance")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> General(GeneralSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("General", "Site.Name", model.SiteName);
                await SaveSettingAsync("General", "Site.Description", model.SiteDescription);
                await SaveSettingAsync("General", "Site.Url", model.SiteUrl);
                await SaveSettingAsync("General", "Site.Timezone", model.SiteTimezone);
                await SaveSettingAsync("General", "Site.Language", model.DefaultLanguage);
                await SaveSettingAsync("General", "Site.DateFormat", model.DateFormat);
                await SaveSettingAsync("General", "Site.TimeFormat", model.TimeFormat);
                await SaveSettingAsync("General", "Site.PageSize", model.PageSize.ToString());
                await SaveSettingAsync("General", "Site.EnableRegistration", model.EnableRegistration.ToString());
                await SaveSettingAsync("General", "Site.RequireEmailConfirmation", model.RequireEmailConfirmation.ToString());
                await SaveSettingAsync("General", "Site.MaintenanceMode", model.MaintenanceMode.ToString());
                await SaveSettingAsync("General", "Site.MaintenanceMessage", model.MaintenanceMessage);

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated general settings");

                TempData["Success"] = "General settings updated successfully.";
                return RedirectToAction(nameof(General));
            }

            return View(model);
        }

        public async Task<IActionResult> Email()
        {
            var settings = await GetSettingsByCategoryAsync("Email");
            var viewModel = new EmailSettingsViewModel
            {
                SmtpHost = GetSettingValue(settings, "Email.SmtpHost", "smtp.gmail.com"),
                SmtpPort = int.Parse(GetSettingValue(settings, "Email.SmtpPort", "587")),
                SmtpUsername = GetSettingValue(settings, "Email.SmtpUsername", ""),
                SmtpPassword = GetSettingValue(settings, "Email.SmtpPassword", ""),
                EnableSsl = bool.Parse(GetSettingValue(settings, "Email.EnableSsl", "true")),
                FromEmail = GetSettingValue(settings, "Email.FromEmail", "noreply@example.com"),
                FromName = GetSettingValue(settings, "Email.FromName", "ModularHost"),
                ReplyToEmail = GetSettingValue(settings, "Email.ReplyToEmail", ""),
                EnableEmailNotifications = bool.Parse(GetSettingValue(settings, "Email.EnableNotifications", "true"))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Email(EmailSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Email", "Email.SmtpHost", model.SmtpHost);
                await SaveSettingAsync("Email", "Email.SmtpPort", model.SmtpPort.ToString());
                await SaveSettingAsync("Email", "Email.SmtpUsername", model.SmtpUsername);
                
                // Only update password if it was changed
                if (!string.IsNullOrEmpty(model.SmtpPassword) && model.SmtpPassword != "********")
                {
                    await SaveSettingAsync("Email", "Email.SmtpPassword", model.SmtpPassword);
                }
                
                await SaveSettingAsync("Email", "Email.EnableSsl", model.EnableSsl.ToString());
                await SaveSettingAsync("Email", "Email.FromEmail", model.FromEmail);
                await SaveSettingAsync("Email", "Email.FromName", model.FromName);
                await SaveSettingAsync("Email", "Email.ReplyToEmail", model.ReplyToEmail ?? "");
                await SaveSettingAsync("Email", "Email.EnableNotifications", model.EnableEmailNotifications.ToString());

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated email settings");

                TempData["Success"] = "Email settings updated successfully.";
                return RedirectToAction(nameof(Email));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(string testEmailAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(testEmailAddress) || !IsValidEmail(testEmailAddress))
                {
                    return Json(new { success = false, message = "Please provide a valid email address." });
                }

                await _emailService.SendEmailAsync(
                    testEmailAddress,
                    "Test Email from ModularHost",
                    "<h3>Test Email</h3><p>This is a test email from your ModularHost application. If you received this email, your email settings are configured correctly.</p>"
                );

                return Json(new { success = true, message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to send test email: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Security()
        {
            var settings = await GetSettingsByCategoryAsync("Security");
            var viewModel = new SecuritySettingsViewModel
            {
                PasswordMinLength = int.Parse(GetSettingValue(settings, "Security.PasswordMinLength", "8")),
                PasswordRequireDigit = bool.Parse(GetSettingValue(settings, "Security.PasswordRequireDigit", "true")),
                PasswordRequireLowercase = bool.Parse(GetSettingValue(settings, "Security.PasswordRequireLowercase", "true")),
                PasswordRequireUppercase = bool.Parse(GetSettingValue(settings, "Security.PasswordRequireUppercase", "true")),
                PasswordRequireNonAlphanumeric = bool.Parse(GetSettingValue(settings, "Security.PasswordRequireNonAlphanumeric", "true")),
                LockoutEnabled = bool.Parse(GetSettingValue(settings, "Security.LockoutEnabled", "true")),
                MaxFailedAccessAttempts = int.Parse(GetSettingValue(settings, "Security.MaxFailedAccessAttempts", "5")),
                LockoutDurationMinutes = int.Parse(GetSettingValue(settings, "Security.LockoutDurationMinutes", "15")),
                EnableTwoFactorAuthentication = bool.Parse(GetSettingValue(settings, "Security.EnableTwoFactor", "false")),
                SessionTimeoutMinutes = int.Parse(GetSettingValue(settings, "Security.SessionTimeoutMinutes", "30")),
                RequireHttps = bool.Parse(GetSettingValue(settings, "Security.RequireHttps", "true")),
                EnableAuditLog = bool.Parse(GetSettingValue(settings, "Security.EnableAuditLog", "true")),
                EnableIpWhitelisting = bool.Parse(GetSettingValue(settings, "Security.EnableIpWhitelisting", "false")),
                IpWhitelist = GetSettingValue(settings, "Security.IpWhitelist", "")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Security(SecuritySettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Security", "Security.PasswordMinLength", model.PasswordMinLength.ToString());
                await SaveSettingAsync("Security", "Security.PasswordRequireDigit", model.PasswordRequireDigit.ToString());
                await SaveSettingAsync("Security", "Security.PasswordRequireLowercase", model.PasswordRequireLowercase.ToString());
                await SaveSettingAsync("Security", "Security.PasswordRequireUppercase", model.PasswordRequireUppercase.ToString());
                await SaveSettingAsync("Security", "Security.PasswordRequireNonAlphanumeric", model.PasswordRequireNonAlphanumeric.ToString());
                await SaveSettingAsync("Security", "Security.LockoutEnabled", model.LockoutEnabled.ToString());
                await SaveSettingAsync("Security", "Security.MaxFailedAccessAttempts", model.MaxFailedAccessAttempts.ToString());
                await SaveSettingAsync("Security", "Security.LockoutDurationMinutes", model.LockoutDurationMinutes.ToString());
                await SaveSettingAsync("Security", "Security.EnableTwoFactor", model.EnableTwoFactorAuthentication.ToString());
                await SaveSettingAsync("Security", "Security.SessionTimeoutMinutes", model.SessionTimeoutMinutes.ToString());
                await SaveSettingAsync("Security", "Security.RequireHttps", model.RequireHttps.ToString());
                await SaveSettingAsync("Security", "Security.EnableAuditLog", model.EnableAuditLog.ToString());
                await SaveSettingAsync("Security", "Security.EnableIpWhitelisting", model.EnableIpWhitelisting.ToString());
                await SaveSettingAsync("Security", "Security.IpWhitelist", model.IpWhitelist ?? "");

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated security settings");

                TempData["Success"] = "Security settings updated successfully.";
                return RedirectToAction(nameof(Security));
            }

            return View(model);
        }

        private async Task<Dictionary<string, Setting>> GetSettingsByCategoryAsync(string category)
        {
            return await _context.Set<Setting>()
                .Where(s => s.Category == category)
                .ToDictionaryAsync(s => s.Key, s => s);
        }

        private string GetSettingValue(Dictionary<string, Setting> settings, string key, string defaultValue)
        {
            return settings.ContainsKey(key) ? settings[key].Value : defaultValue;
        }

        private async Task SaveSettingAsync(string category, string key, string value)
        {
            var setting = await _context.Set<Setting>()
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Category = category,
                    Description = "",
                    IsPublic = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Set<Setting>().Add(setting);
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                _context.Set<Setting>().Update(setting);
            }
        }

        public async Task<IActionResult> Appearance()
        {
            var settings = await GetSettingsByCategoryAsync("Appearance");
            var viewModel = new AppearanceSettingsViewModel
            {
                Theme = GetSettingValue(settings, "Appearance.Theme", "default"),
                EnableDarkMode = bool.Parse(GetSettingValue(settings, "Appearance.EnableDarkMode", "true")),
                PrimaryColor = GetSettingValue(settings, "Appearance.PrimaryColor", "#3b82f6"),
                SecondaryColor = GetSettingValue(settings, "Appearance.SecondaryColor", "#64748b"),
                LogoUrl = GetSettingValue(settings, "Appearance.LogoUrl", ""),
                FaviconUrl = GetSettingValue(settings, "Appearance.FaviconUrl", ""),
                CustomCss = GetSettingValue(settings, "Appearance.CustomCss", ""),
                CustomJavaScript = GetSettingValue(settings, "Appearance.CustomJavaScript", ""),
                FooterText = GetSettingValue(settings, "Appearance.FooterText", "© 2024 ModularHost. All rights reserved."),
                ShowFooter = bool.Parse(GetSettingValue(settings, "Appearance.ShowFooter", "true"))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Appearance(AppearanceSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Appearance", "Appearance.Theme", model.Theme);
                await SaveSettingAsync("Appearance", "Appearance.EnableDarkMode", model.EnableDarkMode.ToString());
                await SaveSettingAsync("Appearance", "Appearance.PrimaryColor", model.PrimaryColor);
                await SaveSettingAsync("Appearance", "Appearance.SecondaryColor", model.SecondaryColor);
                await SaveSettingAsync("Appearance", "Appearance.LogoUrl", model.LogoUrl ?? "");
                await SaveSettingAsync("Appearance", "Appearance.FaviconUrl", model.FaviconUrl ?? "");
                await SaveSettingAsync("Appearance", "Appearance.CustomCss", model.CustomCss ?? "");
                await SaveSettingAsync("Appearance", "Appearance.CustomJavaScript", model.CustomJavaScript ?? "");
                await SaveSettingAsync("Appearance", "Appearance.FooterText", model.FooterText);
                await SaveSettingAsync("Appearance", "Appearance.ShowFooter", model.ShowFooter.ToString());

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated appearance settings");

                TempData["Success"] = "Appearance settings updated successfully.";
                return RedirectToAction(nameof(Appearance));
            }

            return View(model);
        }

        public async Task<IActionResult> Notifications()
        {
            var settings = await GetSettingsByCategoryAsync("Notifications");
            var viewModel = new NotificationSettingsViewModel
            {
                EnableEmailNotifications = bool.Parse(GetSettingValue(settings, "Notifications.EnableEmail", "true")),
                EnablePushNotifications = bool.Parse(GetSettingValue(settings, "Notifications.EnablePush", "false")),
                EnableInAppNotifications = bool.Parse(GetSettingValue(settings, "Notifications.EnableInApp", "true")),
                NotificationRetentionDays = int.Parse(GetSettingValue(settings, "Notifications.RetentionDays", "30")),
                SendWelcomeEmail = bool.Parse(GetSettingValue(settings, "Notifications.SendWelcome", "true")),
                SendPasswordResetEmail = bool.Parse(GetSettingValue(settings, "Notifications.SendPasswordReset", "true")),
                SendAccountLockedEmail = bool.Parse(GetSettingValue(settings, "Notifications.SendAccountLocked", "true")),
                AdminAlertEmail = GetSettingValue(settings, "Notifications.AdminAlertEmail", ""),
                SendDailySummary = bool.Parse(GetSettingValue(settings, "Notifications.SendDailySummary", "false")),
                SendWeeklyReport = bool.Parse(GetSettingValue(settings, "Notifications.SendWeeklyReport", "false"))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Notifications(NotificationSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Notifications", "Notifications.EnableEmail", model.EnableEmailNotifications.ToString());
                await SaveSettingAsync("Notifications", "Notifications.EnablePush", model.EnablePushNotifications.ToString());
                await SaveSettingAsync("Notifications", "Notifications.EnableInApp", model.EnableInAppNotifications.ToString());
                await SaveSettingAsync("Notifications", "Notifications.RetentionDays", model.NotificationRetentionDays.ToString());
                await SaveSettingAsync("Notifications", "Notifications.SendWelcome", model.SendWelcomeEmail.ToString());
                await SaveSettingAsync("Notifications", "Notifications.SendPasswordReset", model.SendPasswordResetEmail.ToString());
                await SaveSettingAsync("Notifications", "Notifications.SendAccountLocked", model.SendAccountLockedEmail.ToString());
                await SaveSettingAsync("Notifications", "Notifications.AdminAlertEmail", model.AdminAlertEmail ?? "");
                await SaveSettingAsync("Notifications", "Notifications.SendDailySummary", model.SendDailySummary.ToString());
                await SaveSettingAsync("Notifications", "Notifications.SendWeeklyReport", model.SendWeeklyReport.ToString());

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated notification settings");

                TempData["Success"] = "Notification settings updated successfully.";
                return RedirectToAction(nameof(Notifications));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestNotification(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                {
                    return Json(new { success = false, message = "Please provide a valid email address." });
                }

                await _emailService.SendEmailAsync(
                    email,
                    "Test Notification from ModularHost",
                    "<h3>Test Notification</h3><p>This is a test notification from your ModularHost application. Your notification settings are working correctly!</p>"
                );

                return Json(new { success = true, message = "Test notification sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to send test notification: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Privacy()
        {
            var settings = await GetSettingsByCategoryAsync("Privacy");
            var viewModel = new PrivacySettingsViewModel
            {
                EnableCookieConsent = bool.Parse(GetSettingValue(settings, "Privacy.EnableCookieConsent", "true")),
                CookieConsentMessage = GetSettingValue(settings, "Privacy.CookieConsentMessage", "This website uses cookies to ensure you get the best experience."),
                EnableGdprCompliance = bool.Parse(GetSettingValue(settings, "Privacy.EnableGdprCompliance", "true")),
                PrivacyPolicyUrl = GetSettingValue(settings, "Privacy.PrivacyPolicyUrl", ""),
                TermsOfServiceUrl = GetSettingValue(settings, "Privacy.TermsOfServiceUrl", ""),
                DataRetentionDays = int.Parse(GetSettingValue(settings, "Privacy.DataRetentionDays", "365")),
                AllowUserDataExport = bool.Parse(GetSettingValue(settings, "Privacy.AllowUserDataExport", "true")),
                AllowUserDataDeletion = bool.Parse(GetSettingValue(settings, "Privacy.AllowUserDataDeletion", "true")),
                CollectAnonymousStatistics = bool.Parse(GetSettingValue(settings, "Privacy.CollectAnonymousStatistics", "true")),
                EnableThirdPartyAnalytics = bool.Parse(GetSettingValue(settings, "Privacy.EnableThirdPartyAnalytics", "false")),
                AnalyticsTrackingId = GetSettingValue(settings, "Privacy.AnalyticsTrackingId", "")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Privacy(PrivacySettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Privacy", "Privacy.EnableCookieConsent", model.EnableCookieConsent.ToString());
                await SaveSettingAsync("Privacy", "Privacy.CookieConsentMessage", model.CookieConsentMessage);
                await SaveSettingAsync("Privacy", "Privacy.EnableGdprCompliance", model.EnableGdprCompliance.ToString());
                await SaveSettingAsync("Privacy", "Privacy.PrivacyPolicyUrl", model.PrivacyPolicyUrl ?? "");
                await SaveSettingAsync("Privacy", "Privacy.TermsOfServiceUrl", model.TermsOfServiceUrl ?? "");
                await SaveSettingAsync("Privacy", "Privacy.DataRetentionDays", model.DataRetentionDays.ToString());
                await SaveSettingAsync("Privacy", "Privacy.AllowUserDataExport", model.AllowUserDataExport.ToString());
                await SaveSettingAsync("Privacy", "Privacy.AllowUserDataDeletion", model.AllowUserDataDeletion.ToString());
                await SaveSettingAsync("Privacy", "Privacy.CollectAnonymousStatistics", model.CollectAnonymousStatistics.ToString());
                await SaveSettingAsync("Privacy", "Privacy.EnableThirdPartyAnalytics", model.EnableThirdPartyAnalytics.ToString());
                await SaveSettingAsync("Privacy", "Privacy.AnalyticsTrackingId", model.AnalyticsTrackingId ?? "");

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated privacy settings");

                TempData["Success"] = "Privacy settings updated successfully.";
                return RedirectToAction(nameof(Privacy));
            }

            return View(model);
        }

        public async Task<IActionResult> Advanced()
        {
            var settings = await GetSettingsByCategoryAsync("Advanced");
            var viewModel = new AdvancedSettingsViewModel
            {
                EnableCaching = bool.Parse(GetSettingValue(settings, "Advanced.EnableCaching", "true")),
                CacheDurationMinutes = int.Parse(GetSettingValue(settings, "Advanced.CacheDurationMinutes", "60")),
                EnableCdn = bool.Parse(GetSettingValue(settings, "Advanced.EnableCdn", "false")),
                CdnUrl = GetSettingValue(settings, "Advanced.CdnUrl", ""),
                EnableApi = bool.Parse(GetSettingValue(settings, "Advanced.EnableApi", "true")),
                ApiRateLimit = int.Parse(GetSettingValue(settings, "Advanced.ApiRateLimit", "60")),
                EnableWebSockets = bool.Parse(GetSettingValue(settings, "Advanced.EnableWebSockets", "true")),
                EnableBackgroundJobs = bool.Parse(GetSettingValue(settings, "Advanced.EnableBackgroundJobs", "true")),
                JobRetentionDays = int.Parse(GetSettingValue(settings, "Advanced.JobRetentionDays", "7")),
                EnableDevelopmentMode = bool.Parse(GetSettingValue(settings, "Advanced.EnableDevelopmentMode", "false")),
                EnableDebugLogging = bool.Parse(GetSettingValue(settings, "Advanced.EnableDebugLogging", "false")),
                LogLevel = GetSettingValue(settings, "Advanced.LogLevel", "Information"),
                MaxUploadSizeMb = int.Parse(GetSettingValue(settings, "Advanced.MaxUploadSizeMb", "10")),
                AllowedFileExtensions = GetSettingValue(settings, "Advanced.AllowedFileExtensions", ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Advanced(AdvancedSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSettingAsync("Advanced", "Advanced.EnableCaching", model.EnableCaching.ToString());
                await SaveSettingAsync("Advanced", "Advanced.CacheDurationMinutes", model.CacheDurationMinutes.ToString());
                await SaveSettingAsync("Advanced", "Advanced.EnableCdn", model.EnableCdn.ToString());
                await SaveSettingAsync("Advanced", "Advanced.CdnUrl", model.CdnUrl ?? "");
                await SaveSettingAsync("Advanced", "Advanced.EnableApi", model.EnableApi.ToString());
                await SaveSettingAsync("Advanced", "Advanced.ApiRateLimit", model.ApiRateLimit.ToString());
                await SaveSettingAsync("Advanced", "Advanced.EnableWebSockets", model.EnableWebSockets.ToString());
                await SaveSettingAsync("Advanced", "Advanced.EnableBackgroundJobs", model.EnableBackgroundJobs.ToString());
                await SaveSettingAsync("Advanced", "Advanced.JobRetentionDays", model.JobRetentionDays.ToString());
                await SaveSettingAsync("Advanced", "Advanced.EnableDevelopmentMode", model.EnableDevelopmentMode.ToString());
                await SaveSettingAsync("Advanced", "Advanced.EnableDebugLogging", model.EnableDebugLogging.ToString());
                await SaveSettingAsync("Advanced", "Advanced.LogLevel", model.LogLevel);
                await SaveSettingAsync("Advanced", "Advanced.MaxUploadSizeMb", model.MaxUploadSizeMb.ToString());
                await SaveSettingAsync("Advanced", "Advanced.AllowedFileExtensions", model.AllowedFileExtensions);

                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Settings", "Update", "Updated advanced settings");

                TempData["Success"] = "Advanced settings updated successfully.";
                return RedirectToAction(nameof(Advanced));
            }

            return View(model);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}