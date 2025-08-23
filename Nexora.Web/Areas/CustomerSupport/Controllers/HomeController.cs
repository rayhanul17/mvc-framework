using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Controllers;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class HomeController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<HomeController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // Check if user is support agent based on actual permissions, not hardcoded roles
        var isAgent = await IsUserSupportAgent(currentUser);
        
        ViewBag.User = currentUser;
        ViewBag.IsAgent = isAgent;
        
        // Get quick stats for the user
        if (isAgent || currentUser.IsSuperAdmin)
        {
            ViewBag.OpenTickets = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.New);
            
            ViewBag.UnassignedTickets = await _context.Tickets
                .CountAsync(t => t.AssignedToId == null && t.Status != TicketStatus.Closed);
            
            ViewBag.MyTickets = await _context.Tickets
                .CountAsync(t => t.AssignedToId == currentUser.Id && t.Status != TicketStatus.Closed);
        }
        else
        {
            // For regular customers, show their ticket count
            ViewBag.MyTickets = await _context.Tickets
                .CountAsync(t => t.CustomerId == currentUser.Id);
            
            ViewBag.OpenTickets = await _context.Tickets
                .CountAsync(t => t.CustomerId == currentUser.Id && 
                            (t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved));
        }
        
        return View();
    }
    
    private async Task<bool> IsUserSupportAgent(ApplicationUser user)
    {
        // Check if user has IsSuperAdmin flag
        if (user.IsSuperAdmin)
            return true;
        
        // You can add additional logic here to determine if a user is a support agent
        // For example, check custom claims, permissions table, or other criteria
        // For now, we'll check if they have any role that contains "Support" or "Admin"
        var roles = await _userManager.GetRolesAsync(user);
        return roles.Any(r => r.Contains("Support", StringComparison.OrdinalIgnoreCase) || 
                              r.Contains("Admin", StringComparison.OrdinalIgnoreCase));
    }
}