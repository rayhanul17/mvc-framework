using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Infrastructure.Data;
using DynamicRoleMenuSystem.Web.Controllers;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class AdminController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<AdminController> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReseedSuperAdminMenus()
    {
        try
        {
            _logger.LogInformation("Starting SuperAdmin menu reseeding process");
            
            // Call the comprehensive menu seeder
            await SuperAdminMenuSeeder.SeedAllMenusForSuperAdminAsync(_context, _roleManager);
            
            SetSuccessMessage("SuperAdmin menus have been successfully reseeded with all permissions.");
            _logger.LogInformation("SuperAdmin menu reseeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reseeding SuperAdmin menus");
            SetErrorMessage($"An error occurred while reseeding menus: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReseedAllData()
    {
        try
        {
            _logger.LogInformation("Starting complete database reseeding process");
            
            // Call the main database initializer
            await DbInitializer.InitializeAsync(HttpContext.RequestServices);
            
            // Then ensure SuperAdmin has all menus
            await SuperAdminMenuSeeder.SeedAllMenusForSuperAdminAsync(_context, _roleManager);
            
            SetSuccessMessage("Database has been successfully reseeded with all initial data.");
            _logger.LogInformation("Database reseeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reseeding database");
            SetErrorMessage($"An error occurred while reseeding database: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }
}