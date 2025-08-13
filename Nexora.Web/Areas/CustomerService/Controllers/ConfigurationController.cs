using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Areas.CustomerService.Models;
using Nexora.Web.Controllers;

namespace Nexora.Web.Areas.CustomerService.Controllers;

[Area("CustomerService")]
[Authorize(Roles = "SuperAdmin,Administrator,CustomerSupportAdmin")]
public class ConfigurationController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<ConfigurationController> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new RoleMappingViewModel();
        
        // Get all existing mappings
        var mappings = await _context.CustomerServiceRoleMappings
            .Include(m => m.AspNetRole)
            .OrderBy(m => m.CustomerServiceRole)
            .ToListAsync();
        
        // Define default Customer Service roles
        var defaultRoles = new Dictionary<string, string>
        {
            { "CustomerSupportAdmin", "Admin role for Customer Service area" },
            { "CustomerSupportManager", "Manager role for Customer Service area" },
            { "CustomerSupportAgent", "Agent role for Customer Service area" },
            { "CustomerSupportCustomer", "Default customer role for Customer Service area" }
        };
        
        // Ensure all default roles have mappings
        foreach (var defaultRole in defaultRoles)
        {
            var mapping = mappings.FirstOrDefault(m => m.CustomerServiceRole == defaultRole.Key);
            if (mapping == null)
            {
                // Create a default mapping if it doesn't exist
                viewModel.Mappings.Add(new RoleMappingItem
                {
                    Id = 0,
                    CustomerServiceRole = defaultRole.Key,
                    AspNetRoleName = "",
                    Description = defaultRole.Value,
                    IsActive = false,
                    IsSystemDefault = true
                });
            }
            else
            {
                viewModel.Mappings.Add(new RoleMappingItem
                {
                    Id = mapping.Id,
                    CustomerServiceRole = mapping.CustomerServiceRole,
                    AspNetRoleName = mapping.AspNetRoleName,
                    Description = mapping.Description,
                    IsActive = mapping.IsActive,
                    IsSystemDefault = true
                });
            }
        }
        
        // Get all available ASP.NET roles
        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            })
            .ToListAsync();
        
        viewModel.AvailableRoles = roles;
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMapping(SaveRoleMappingModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data provided" });
        }

        try
        {
            // Check if the ASP.NET role exists
            var roleExists = await _roleManager.RoleExistsAsync(model.AspNetRoleName);
            if (!roleExists)
            {
                return Json(new { success = false, message = "Selected ASP.NET role does not exist" });
            }

            // Check if mapping already exists
            var existingMapping = await _context.CustomerServiceRoleMappings
                .FirstOrDefaultAsync(m => m.CustomerServiceRole == model.CustomerServiceRole);

            if (existingMapping != null)
            {
                // Update existing mapping
                existingMapping.AspNetRoleName = model.AspNetRoleName;
                existingMapping.Description = model.Description;
                existingMapping.IsActive = true;
                existingMapping.UpdatedAt = DateTime.UtcNow;
                
                _context.CustomerServiceRoleMappings.Update(existingMapping);
            }
            else
            {
                // Create new mapping
                var newMapping = new CustomerServiceRoleMapping
                {
                    CustomerServiceRole = model.CustomerServiceRole,
                    AspNetRoleName = model.AspNetRoleName,
                    Description = model.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.CustomerServiceRoleMappings.AddAsync(newMapping);
            }

            await _context.SaveChangesAsync();
            
            SetSuccessMessage($"Role mapping for {model.CustomerServiceRole} has been saved successfully.");
            return Json(new { success = true, message = "Role mapping saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving role mapping");
            return Json(new { success = false, message = "An error occurred while saving the role mapping" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMapping(string customerServiceRole)
    {
        try
        {
            var mapping = await _context.CustomerServiceRoleMappings
                .FirstOrDefaultAsync(m => m.CustomerServiceRole == customerServiceRole);

            if (mapping == null)
            {
                return Json(new { success = false, message = "Mapping not found" });
            }

            _context.CustomerServiceRoleMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            
            SetSuccessMessage($"Role mapping for {customerServiceRole} has been deleted.");
            return Json(new { success = true, message = "Role mapping deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role mapping");
            return Json(new { success = false, message = "An error occurred while deleting the role mapping" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMapping(string customerServiceRole)
    {
        try
        {
            var mapping = await _context.CustomerServiceRoleMappings
                .FirstOrDefaultAsync(m => m.CustomerServiceRole == customerServiceRole);

            if (mapping == null)
            {
                return Json(new { success = false, message = "Mapping not found" });
            }

            mapping.IsActive = !mapping.IsActive;
            mapping.UpdatedAt = DateTime.UtcNow;
            
            _context.CustomerServiceRoleMappings.Update(mapping);
            await _context.SaveChangesAsync();
            
            var status = mapping.IsActive ? "activated" : "deactivated";
            SetSuccessMessage($"Role mapping for {customerServiceRole} has been {status}.");
            return Json(new { success = true, isActive = mapping.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling role mapping");
            return Json(new { success = false, message = "An error occurred while toggling the role mapping" });
        }
    }
}