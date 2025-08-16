using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Models;
using System.Security.Claims;

namespace Nexora.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISiteSettingService _siteSettingService;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<ApplicationUser> userManager,
        ISiteSettingService siteSettingService)
    {
        _logger = logger;
        _userManager = userManager;
        _siteSettingService = siteSettingService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectToDashboard();
        }
        
        return View();
    }

    public IActionResult NotificationDemo()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        return await RedirectToDashboard();
    }

    private async Task<IActionResult> RedirectToDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Check for Super Admin or Administrator first
            if (userRoles.Contains("SuperAdmin") || userRoles.Contains("Administrator"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            
            // Check for Customer Support roles
            if (userRoles.Contains("CustomerSupportAdmin") || 
                userRoles.Contains("CustomerSupportManager") || 
                userRoles.Contains("CustomerSupportAgent"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "CustomerSupport" });
            }
            
            // Check for Customer Service roles (role mapping system)
            if (userRoles.Any(r => r.StartsWith("CustomerService")))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "CustomerService" });
            }
            
            // Default dashboard for regular users
            return RedirectToAction("UserDashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining dashboard redirect for user");
            return RedirectToAction("UserDashboard");
        }
    }

    [Authorize]
    public async Task<IActionResult> UserDashboard()
    {
        var model = await BuildUserDashboardModel();
        return View(model);
    }

    private async Task<UserDashboardViewModel> BuildUserDashboardModel()
    {
        var model = new UserDashboardViewModel();
        
        // Get site settings
        var siteSettingsResult = await _siteSettingService.GetAllSettingsAsync();
        if (siteSettingsResult.IsSuccess && siteSettingsResult.Data != null)
        {
            model.SiteName = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? "Nexora Framework";
            model.SiteSlogan = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "SiteSlogan")?.Value ?? "Dynamic Role-Based System";
            model.WelcomeMessage = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "UserWelcomeMessage")?.Value ?? "Welcome to your dashboard";
        }

        // Get user information
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                model.UserName = user.UserName ?? "";
                model.FullName = user.FullName;
                model.Email = user.Email ?? "";
                model.UserRoles = (await _userManager.GetRolesAsync(user)).ToList();
                model.MemberSince = user.CreatedAt;
            }
        }

        return model;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
