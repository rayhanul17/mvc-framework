using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class ConfigurationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ConfigurationController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ConfigurationViewModel
        {
            // Get ticket categories
            Categories = await _context.Tickets
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(),

            // Get support agents
            SupportAgents = await GetSupportAgentsAsync(),

            // Get ticket priorities with SLA
            PrioritySLAs = GetPrioritySLAs(),

            // Get auto-assignment rules
            AutoAssignmentEnabled = await GetSettingAsync("AutoAssignmentEnabled") == "true",
            DefaultAssignmentMethod = await GetSettingAsync("DefaultAssignmentMethod") ?? "RoundRobin",

            // Get notification settings
            EmailNotificationsEnabled = await GetSettingAsync("EmailNotificationsEnabled") == "true",
            SmsNotificationsEnabled = await GetSettingAsync("SmsNotificationsEnabled") == "true",

            // Get business hours
            BusinessHoursStart = await GetSettingAsync("BusinessHoursStart") ?? "09:00",
            BusinessHoursEnd = await GetSettingAsync("BusinessHoursEnd") ?? "18:00",
            WorkingDays = await GetSettingAsync("WorkingDays") ?? "Monday,Tuesday,Wednesday,Thursday,Friday"
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCategories([FromBody] List<string> categories)
    {
        try
        {
            // Save categories to settings
            await SaveSettingAsync("TicketCategories", string.Join(",", categories));
            return Json(new { success = true, message = "Categories saved successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SavePrioritySLA([FromBody] PrioritySLA sla)
    {
        try
        {
            var key = $"SLA_{sla.Priority}";
            var value = $"{sla.ResponseTimeHours},{sla.ResolutionTimeHours}";
            await SaveSettingAsync(key, value);
            return Json(new { success = true, message = "SLA saved successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveAutoAssignment([FromBody] AutoAssignmentSettings settings)
    {
        try
        {
            await SaveSettingAsync("AutoAssignmentEnabled", settings.Enabled.ToString());
            await SaveSettingAsync("DefaultAssignmentMethod", settings.Method);
            return Json(new { success = true, message = "Auto-assignment settings saved" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveNotificationSettings([FromBody] NotificationSettings settings)
    {
        try
        {
            await SaveSettingAsync("EmailNotificationsEnabled", settings.EmailEnabled.ToString());
            await SaveSettingAsync("SmsNotificationsEnabled", settings.SmsEnabled.ToString());
            return Json(new { success = true, message = "Notification settings saved" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveBusinessHours([FromBody] BusinessHours hours)
    {
        try
        {
            await SaveSettingAsync("BusinessHoursStart", hours.StartTime);
            await SaveSettingAsync("BusinessHoursEnd", hours.EndTime);
            await SaveSettingAsync("WorkingDays", string.Join(",", hours.WorkingDays));
            return Json(new { success = true, message = "Business hours saved" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAgentWorkload()
    {
        var agents = await GetSupportAgentsAsync();
        var workload = new List<object>();

        foreach (var agent in agents)
        {
            var ticketCount = await _context.Tickets
                .Where(t => t.AssignedToId == agent.Id && t.Status != TicketStatus.Closed)
                .CountAsync();

            var avgResponseTime = await _context.Tickets
                .Where(t => t.AssignedToId == agent.Id && t.ResponseTimeHours.HasValue)
                .Select(t => t.ResponseTimeHours!.Value)
                .DefaultIfEmpty(0)
                .AverageAsync();

            workload.Add(new
            {
                agentId = agent.Id,
                agentName = agent.FullName,
                activeTickets = ticketCount,
                avgResponseTime = Math.Round(avgResponseTime, 2)
            });
        }

        return Json(workload);
    }

    private async Task<List<ApplicationUser>> GetSupportAgentsAsync()
    {
        var supportRoles = new[] { "CustomerSupportAgent", "CustomerSupportManager", "CustomerSupportAdmin" };
        var agents = new List<ApplicationUser>();

        foreach (var roleName in supportRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                agents.AddRange(usersInRole);
            }
        }

        return agents.Distinct().ToList();
    }

    private List<PrioritySLA> GetPrioritySLAs()
    {
        return new List<PrioritySLA>
        {
            new PrioritySLA { Priority = "Critical", ResponseTimeHours = 1, ResolutionTimeHours = 4 },
            new PrioritySLA { Priority = "Urgent", ResponseTimeHours = 2, ResolutionTimeHours = 8 },
            new PrioritySLA { Priority = "High", ResponseTimeHours = 4, ResolutionTimeHours = 24 },
            new PrioritySLA { Priority = "Medium", ResponseTimeHours = 8, ResolutionTimeHours = 48 },
            new PrioritySLA { Priority = "Low", ResponseTimeHours = 24, ResolutionTimeHours = 72 }
        };
    }

    private async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == $"CustomerSupport_{key}");
        return setting?.Value;
    }

    private async Task SaveSettingAsync(string key, string value)
    {
        var fullKey = $"CustomerSupport_{key}";
        var setting = await _context.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == fullKey);

        if (setting == null)
        {
            setting = new SiteSetting
            {
                Key = fullKey,
                Value = value,
                Category = SettingCategory.Advanced,
                CreatedAt = DateTime.UtcNow
            };
            _context.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            _context.SiteSettings.Update(setting);
        }

        await _context.SaveChangesAsync();
    }
}

// Supporting classes
public class AutoAssignmentSettings
{
    public bool Enabled { get; set; }
    public string Method { get; set; } = "RoundRobin";
}

public class NotificationSettings
{
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}

public class BusinessHours
{
    public string StartTime { get; set; } = "09:00";
    public string EndTime { get; set; } = "18:00";
    public List<string> WorkingDays { get; set; } = new();
}