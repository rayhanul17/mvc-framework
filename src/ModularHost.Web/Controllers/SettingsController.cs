using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Services.Interfaces;
using MRCMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using MRCMS.Services;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
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