using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerService.Models;
using Nexora.Web.Controllers;
using System.Security.Claims;

namespace Nexora.Web.Areas.CustomerService.Controllers;

[Area("CustomerService")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISiteSettingService _siteSettingService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ISiteSettingService siteSettingService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _userManager = userManager;
        _siteSettingService = siteSettingService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await BuildCustomerServiceDashboardViewModel(userId!);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer service dashboard");
            SetErrorMessage("Error loading dashboard. Please try again.");
            return View(new CustomerServiceDashboardViewModel());
        }
    }

    private async Task<CustomerServiceDashboardViewModel> BuildCustomerServiceDashboardViewModel(string userId)
    {
        var model = new CustomerServiceDashboardViewModel();

        // Get site settings
        var siteSettingsResult = await _siteSettingService.GetAllSettingsAsync();
        if (siteSettingsResult.IsSuccess)
        {
            model.SiteName = siteSettingsResult.Data?.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? "Nexora Framework";
            model.WelcomeMessage = siteSettingsResult.Data?.FirstOrDefault(s => s.Key == "CustomerServiceWelcome")?.Value ?? "Welcome to Customer Service";
        }

        // Role mapping statistics
        model.TotalRoleMappings = await _context.CustomerServiceRoleMappings.CountAsync();
        model.ActiveRoleMappings = await _context.CustomerServiceRoleMappings.CountAsync(rm => rm.IsActive);
        model.InactiveRoleMappings = model.TotalRoleMappings - model.ActiveRoleMappings;

        // Ticket statistics (if tickets table exists)
        try
        {
            model.TotalTickets = await _context.Tickets.CountAsync();
            model.OpenTickets = await _context.Tickets.CountAsync(t => 
                t.Status == TicketStatus.New || 
                t.Status == TicketStatus.Open || 
                t.Status == TicketStatus.InProgress);
            model.MyTickets = await _context.Tickets.CountAsync(t => t.AssignedToId == userId);
            model.ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved);
        }
        catch
        {
            // Tickets table might not exist yet
            model.TotalTickets = 0;
            model.OpenTickets = 0;
            model.MyTickets = 0;
            model.ResolvedTickets = 0;
        }

        // Recent role mappings
        model.RecentRoleMappings = await _context.CustomerServiceRoleMappings
            .OrderByDescending(rm => rm.CreatedAt)
            .Take(5)
            .Select(rm => new RecentRoleMapping
            {
                CustomerServiceRole = rm.CustomerServiceRole,
                AspNetRoleName = rm.AspNetRoleName,
                IsActive = rm.IsActive,
                CreatedAt = rm.CreatedAt,
                Description = rm.Description ?? ""
            })
            .ToListAsync();

        // System roles
        var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId) ?? new ApplicationUser());
        model.UserRoles = roles.ToList();

        return model;
    }

    [HttpGet]
    public async Task<JsonResult> GetRoleMappingStats()
    {
        var stats = new
        {
            totalMappings = await _context.CustomerServiceRoleMappings.CountAsync(),
            activeMappings = await _context.CustomerServiceRoleMappings.CountAsync(rm => rm.IsActive),
            inactiveMappings = await _context.CustomerServiceRoleMappings.CountAsync(rm => !rm.IsActive),
            timestamp = DateTime.UtcNow
        };

        return Json(stats);
    }
}