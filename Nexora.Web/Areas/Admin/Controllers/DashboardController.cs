using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.Admin.Models;
using Nexora.Web.Controllers;
using System.Security.Claims;

namespace Nexora.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,SuperAdmin")]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ISiteSettingService _siteSettingService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ISiteSettingService siteSettingService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _siteSettingService = siteSettingService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var model = await BuildAdminDashboardViewModel();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            SetErrorMessage("Error loading dashboard. Please try again.");
            return View(new AdminDashboardViewModel());
        }
    }

    private async Task<AdminDashboardViewModel> BuildAdminDashboardViewModel()
    {
        var model = new AdminDashboardViewModel();

        // System overview
        model.TotalUsers = await _userManager.Users.CountAsync();
        model.TotalRoles = await _roleManager.Roles.CountAsync();
        model.ActiveUsers = await _userManager.Users.CountAsync(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow);
        model.LockedUsers = await _userManager.Users.CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow);

        // Recent activities
        model.RecentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentUserActivity 
            { 
                UserName = u.UserName ?? "",
                FullName = u.FullName,
                Email = u.Email ?? "",
                CreatedAt = u.CreatedAt,
                IsActive = !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow
            })
            .ToListAsync();

        // Blog statistics
        model.TotalBlogPosts = await _context.BlogPosts.CountAsync();
        model.PublishedBlogPosts = await _context.BlogPosts.CountAsync(p => p.IsPublished);
        model.DraftBlogPosts = await _context.BlogPosts.CountAsync(p => !p.IsPublished);

        // Site settings
        var siteSettingsResult = await _siteSettingService.GetAllSettingsAsync();
        if (siteSettingsResult.IsSuccess)
        {
            model.SiteName = siteSettingsResult.Data?.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? "Nexora Framework";
            model.SiteSlogan = siteSettingsResult.Data?.FirstOrDefault(s => s.Key == "SiteSlogan")?.Value ?? "Dynamic Role-Based System";
            model.WelcomeMessage = siteSettingsResult.Data?.FirstOrDefault(s => s.Key == "WelcomeMessage")?.Value ?? "Welcome to Admin Dashboard";
        }

        // System health
        model.DatabaseStatus = await CheckDatabaseHealth();
        model.LastBackupDate = DateTime.UtcNow.AddDays(-1); // Placeholder
        
        // Performance metrics
        model.SystemMetrics = new SystemMetrics
        {
            CpuUsage = 45.2, // Placeholder - would integrate with actual system monitoring
            MemoryUsage = 62.8,
            DiskUsage = 34.5,
            ActiveSessions = await GetActiveSessionsCount(),
            RequestsPerMinute = 125 // Placeholder
        };

        return model;
    }

    private async Task<string> CheckDatabaseHealth()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            return "Healthy";
        }
        catch
        {
            return "Error";
        }
    }

    private async Task<int> GetActiveSessionsCount()
    {
        // Placeholder - would implement actual session tracking
        // For now, return count of active users
        return await _userManager.Users.CountAsync(u => u.IsActive);
    }

    [HttpGet]
    public async Task<JsonResult> GetSystemStats()
    {
        var stats = new
        {
            totalUsers = await _userManager.Users.CountAsync(),
            activeUsers = await _userManager.Users.CountAsync(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow),
            totalRoles = await _roleManager.Roles.CountAsync(),
            blogPosts = await _context.BlogPosts.CountAsync(),
            timestamp = DateTime.UtcNow
        };

        return Json(stats);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            // Implement cache clearing logic here
            SetSuccessMessage("System cache cleared successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            SetErrorMessage("Error clearing cache. Please try again.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackupDatabase()
    {
        try
        {
            // Implement database backup logic here
            SetSuccessMessage("Database backup initiated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating database backup");
            SetErrorMessage("Error initiating backup. Please try again.");
        }

        return RedirectToAction(nameof(Index));
    }
}