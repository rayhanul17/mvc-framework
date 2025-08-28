using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Models;

namespace Nexora.Web.Controllers;

[Authorize]
public class RolePermissionController : BaseController
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUrlAuthorizationService _authorizationService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolePermissionController> _logger;
    
    public RolePermissionController(
        RoleManager<ApplicationRole> roleManager,
        IUrlAuthorizationService authorizationService,
        ApplicationDbContext context,
        ILogger<RolePermissionController> logger)
    {
        _roleManager = roleManager;
        _authorizationService = authorizationService;
        _context = context;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
    }
    
    public async Task<IActionResult> Create()
    {
        return View("CreateEdit", new RolePermissionViewModel());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RolePermissionViewModel model)
    {
        if (!ModelState.IsValid)
            return View("CreateEdit", model);
        
        try
        {
            // Create role
            var role = new ApplicationRole
            {
                Name = model.RoleName,
                Description = model.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View("CreateEdit", model);
            }
            
            // Add URL permissions
            if (!string.IsNullOrWhiteSpace(model.PermissionUrls))
            {
                var urls = model.PermissionUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var url in urls)
                {
                    await _authorizationService.AddRolePermissionAsync(role.Id, url.Trim());
                }
            }
            
            SetSuccessMessage("Role created successfully with permissions");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            SetErrorMessage("An error occurred while creating the role");
            return View("CreateEdit", model);
        }
    }
    
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();
        
        // Get role permissions
        var permissions = await _context.RoleUrlPermissions
            .Where(rp => rp.RoleId == id && rp.IsActive)
            .Select(rp => rp.Url)
            .ToListAsync();
        
        var model = new RolePermissionViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description,
            PermissionUrls = string.Join(", ", permissions),
            CurrentPermissions = permissions
        };
        
        return View("CreateEdit", model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RolePermissionViewModel model)
    {
        if (!ModelState.IsValid)
            return View("CreateEdit", model);
        
        try
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
                return NotFound();
            
            // Update role
            role.Name = model.RoleName;
            role.Description = model.Description;
            role.UpdatedAt = DateTime.UtcNow;
            
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View("CreateEdit", model);
            }
            
            // Update permissions - Remove all existing and add new ones
            var existingPermissions = await _context.RoleUrlPermissions
                .Where(rp => rp.RoleId == model.RoleId)
                .ToListAsync();
            
            _context.RoleUrlPermissions.RemoveRange(existingPermissions);
            
            // Add new permissions
            if (!string.IsNullOrWhiteSpace(model.PermissionUrls))
            {
                var urls = model.PermissionUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var url in urls)
                {
                    var permission = new RoleUrlPermission
                    {
                        RoleId = role.Id,
                        Url = url.Trim().ToLower(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                    _context.RoleUrlPermissions.Add(permission);
                }
            }
            
            await _context.SaveChangesAsync();
            
            SetSuccessMessage("Role updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            SetErrorMessage("An error occurred while updating the role");
            return View("CreateEdit", model);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetRolePermissions(string id)
    {
        var permissions = await _context.RoleUrlPermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => new 
            {
                url = rp.Url,
                description = rp.Description,
                isActive = rp.IsActive
            })
            .ToListAsync();
        
        return Json(permissions);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPermission(string roleId, string url)
    {
        try
        {
            var success = await _authorizationService.AddRolePermissionAsync(roleId, url);
            return Json(new { success, message = success ? "Permission added" : "Permission already exists" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> RemovePermission(string roleId, string url)
    {
        try
        {
            var success = await _authorizationService.RemoveRolePermissionAsync(roleId, url);
            return Json(new { success, message = success ? "Permission removed" : "Permission not found" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();
        
        return View(role);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            
            // Remove all permissions for this role
            var permissions = await _context.RoleUrlPermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();
            
            _context.RoleUrlPermissions.RemoveRange(permissions);
            await _context.SaveChangesAsync();
            
            // Delete role
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                SetErrorMessage("Failed to delete role");
                return View(role);
            }
            
            SetSuccessMessage("Role deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role");
            SetErrorMessage("An error occurred while deleting the role");
            return RedirectToAction(nameof(Index));
        }
    }
}