using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Services;
using Nexora.Core.Constants;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerSupport.Models;
using Nexora.Web.Controllers;

namespace Nexora.Web.Areas.CustomerSupport.Controllers;

[Area("CustomerSupport")]
[Authorize]
public class ConfigurationController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRoleAuthorizationService _roleAuthService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IRoleAuthorizationService roleAuthService,
        ILogger<ConfigurationController> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _roleAuthService = roleAuthService;
        _logger = logger;
    }

    // GET: CustomerSupport/Configuration
    public async Task<IActionResult> Index()
    {
        // Check if user has admin rights
        if (!await _roleAuthService.IsAdminAsync(User))
        {
            SetErrorMessage("You don't have permission to access this page.");
            return RedirectToAction("Index", "Dashboard");
        }

        var model = new ConfigurationViewModel
        {
            RoleMappings = await GetRoleMappingsAsync(),
            AvailableRoles = await GetAvailableRolesAsync(),
            TicketPriorities = await GetTicketPrioritiesAsync(),
            TicketStatuses = await GetTicketStatusesAsync()
        };

        return View(model);
    }

    // POST: CustomerSupport/Configuration/UpdateRoleMapping
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoleMapping(string customerServiceRole, string aspNetRoleId)
    {
        if (!await _roleAuthService.IsAdminAsync(User))
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        try
        {
            var mapping = await _context.CustomerServiceRoleMappings
                .FirstOrDefaultAsync(m => m.CustomerServiceRole == customerServiceRole);

            if (mapping == null)
            {
                return Json(new { success = false, message = "Role mapping not found" });
            }

            mapping.AspNetRoleId = aspNetRoleId;
            mapping.UpdatedAt = DateTime.UtcNow;

            _context.Update(mapping);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role mapping updated: {CustomerServiceRole} -> {AspNetRoleId}", 
                customerServiceRole, aspNetRoleId);

            return Json(new { success = true, message = "Role mapping updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role mapping");
            return Json(new { success = false, message = "An error occurred while updating the role mapping" });
        }
    }

    // POST: CustomerSupport/Configuration/AddRoleMapping
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoleMapping(string customerServiceRole, string aspNetRoleId)
    {
        if (!await _roleAuthService.IsAdminAsync(User))
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        try
        {
            // Check if mapping already exists
            var existingMapping = await _context.CustomerServiceRoleMappings
                .AnyAsync(m => m.CustomerServiceRole == customerServiceRole);

            if (existingMapping)
            {
                return Json(new { success = false, message = "This role mapping already exists" });
            }

            var mapping = new CustomerServiceRoleMapping
            {
                CustomerServiceRole = customerServiceRole,
                AspNetRoleId = aspNetRoleId,
                Description = $"Mapping for {customerServiceRole}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerServiceRoleMappings.Add(mapping);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New role mapping created: {CustomerServiceRole} -> {AspNetRoleId}", 
                customerServiceRole, aspNetRoleId);

            return Json(new { success = true, message = "Role mapping created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role mapping");
            return Json(new { success = false, message = "An error occurred while creating the role mapping" });
        }
    }

    // POST: CustomerSupport/Configuration/DeleteRoleMapping
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoleMapping(int id)
    {
        if (!await _roleAuthService.IsSuperAdminAsync(User))
        {
            return Json(new { success = false, message = "Only SuperAdmin can delete role mappings" });
        }

        try
        {
            var mapping = await _context.CustomerServiceRoleMappings.FindAsync(id);
            if (mapping == null)
            {
                return Json(new { success = false, message = "Role mapping not found" });
            }

            _context.CustomerServiceRoleMappings.Remove(mapping);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role mapping deleted: {CustomerServiceRole}", mapping.CustomerServiceRole);

            return Json(new { success = true, message = "Role mapping deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role mapping");
            return Json(new { success = false, message = "An error occurred while deleting the role mapping" });
        }
    }

    // GET: CustomerSupport/Configuration/GetStatistics
    public async Task<IActionResult> GetStatistics()
    {
        if (!await _roleAuthService.IsAdminAsync(User))
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var stats = new
        {
            TotalTickets = await _context.Tickets.CountAsync(),
            OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open),
            InProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress),
            ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved),
            TotalAgents = await _userManager.GetUsersInRoleAsync(RoleConstants.CustomerSupportAgent)
                .ContinueWith(t => t.Result.Count),
            TotalCustomers = await _userManager.GetUsersInRoleAsync(RoleConstants.CustomerSupportCustomer)
                .ContinueWith(t => t.Result.Count)
        };

        return Json(new { success = true, data = stats });
    }

    #region Private Methods

    private async Task<List<RoleMappingViewModel>> GetRoleMappingsAsync()
    {
        var mappings = await _context.CustomerServiceRoleMappings
            .Include(m => m.AspNetRole)
            .OrderBy(m => m.CustomerServiceRole)
            .Select(m => new RoleMappingViewModel
            {
                Id = m.Id,
                CustomerServiceRole = m.CustomerServiceRole,
                AspNetRoleId = m.AspNetRoleId,
                AspNetRoleName = m.AspNetRole != null ? m.AspNetRole.Name : "Not Mapped",
                Description = m.Description,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .ToListAsync();

        return mappings;
    }

    private async Task<List<ApplicationRole>> GetAvailableRolesAsync()
    {
        return await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    private async Task<List<string>> GetTicketPrioritiesAsync()
    {
        return await Task.FromResult(Enum.GetNames(typeof(TicketPriority)).ToList());
    }

    private async Task<List<string>> GetTicketStatusesAsync()
    {
        return await Task.FromResult(Enum.GetNames(typeof(TicketStatus)).ToList());
    }

    #endregion
}