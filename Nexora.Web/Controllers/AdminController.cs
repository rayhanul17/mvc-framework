using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Controllers;

namespace Nexora.Web.Controllers;

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

    [HttpGet]
    public IActionResult Documentation()
    {
        // Check if user is SuperAdmin
        var user = HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReseedData()
    {
        try
        {
            _logger.LogInformation("Starting database reseeding process");
            
            // Call the main database initializer
            await DbInitializer.InitializeAsync(HttpContext.RequestServices);
            
            SetSuccessMessage("Database has been successfully reseeded with initial data.");
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