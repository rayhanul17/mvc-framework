using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Attributes;
using Nexora.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Application.Services;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.Controllers;

[DynamicPermissionAuthorize]
public class PermissionController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionAuditService _auditService;
    private readonly ICacheManagementService _cacheManagement;

    public PermissionController(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IPermissionAuditService auditService,
        ICacheManagementService cacheManagement)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _auditService = auditService;
        _cacheManagement = cacheManagement;
    }

    public async Task<IActionResult> Index()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Area)
            .ThenBy(p => p.Controller)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return View(permissions);
    }

    public async Task<IActionResult> RolePermissions()
    {
        var roles = await _roleManager.Roles
            .Where(r => r.Name != "SuperAdmin") // SuperAdmin has all permissions
            .OrderBy(r => r.Name)
            .ToListAsync();

        var rolePermissions = new List<RolePermissionViewModel>();

        foreach (var role in roles)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();

            rolePermissions.Add(new RolePermissionViewModel
            {
                Role = role,
                Permissions = permissions
            });
        }

        ViewBag.AllPermissions = await _context.Permissions
            .OrderBy(p => p.Area)
            .ThenBy(p => p.Controller)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return View(rolePermissions);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, List<int> permissionIds)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Json(new { success = false, message = "Role not found" });
        }

        // Don't allow modifying SuperAdmin permissions
        if (role.Name == "SuperAdmin")
        {
            return Json(new { success = false, message = "Cannot modify SuperAdmin permissions" });
        }

        try
        {
            // Get existing permissions for audit logging
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            var oldPermissionIds = existingPermissions.Select(p => p.PermissionId).ToList();

            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            if (permissionIds != null && permissionIds.Any())
            {
                var newPermissions = permissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                });

                await _context.RolePermissions.AddRangeAsync(newPermissions);
            }

            await _context.SaveChangesAsync();

            // Log the permission change
            await _auditService.LogRolePermissionChangeAsync(
                roleId, 
                role.Name ?? "Unknown", 
                oldPermissionIds, 
                permissionIds ?? new List<int>(), 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System"
            );

            // Clear cache after permission update
            _cacheManagement.ClearAllPermissionCaches();

            SetSuccessMessage($"Permissions updated successfully for role: {role.Name}");
            return Json(new { success = true, message = "Permissions updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error updating permissions: {ex.Message}" });
        }
    }

    public async Task<IActionResult> UserPermissions()
    {
        var users = await _userManager.Users
            .Where(u => !u.IsSuperAdmin) // SuperAdmin has all permissions
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var userPermissions = new List<UserPermissionViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleIds = await _context.Roles
                .Where(r => roles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            var permissions = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();

            userPermissions.Add(new UserPermissionViewModel
            {
                User = user,
                Roles = roles.ToList(),
                Permissions = permissions
            });
        }

        return View(userPermissions);
    }

    public async Task<IActionResult> Create()
    {
        var model = new PermissionCreateViewModel
        {
            Areas = GetDistinctAreas(),
            Controllers = new List<SelectListItem>(),
            Actions = new List<SelectListItem>()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PermissionCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if permission already exists
            var exists = await _context.Permissions
                .AnyAsync(p => p.Area == model.Area && 
                              p.Controller == model.Controller && 
                              p.Action == model.Action);

            if (exists)
            {
                ModelState.AddModelError("", "This permission already exists");
                model.Areas = GetDistinctAreas();
                return View(model);
            }

            var permission = new Permission
            {
                Name = model.Name,
                Description = model.Description,
                Area = model.Area,
                Controller = model.Controller,
                Action = model.Action,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            // Clear cache after permission creation
            _cacheManagement.ClearAllPermissionCaches();

            SetSuccessMessage("Permission created successfully");
            return RedirectToAction(nameof(Index));
        }

        model.Areas = GetDistinctAreas();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound();
        }

        var model = new PermissionEditViewModel
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Area = permission.Area,
            Controller = permission.Controller,
            Action = permission.Action,
            IsActive = permission.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PermissionEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var permission = await _context.Permissions.FindAsync(model.Id);
            if (permission == null)
            {
                return NotFound();
            }

            permission.Name = model.Name;
            permission.Description = model.Description;
            permission.IsActive = model.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.ModifiedBy = User.Identity?.Name ?? "System";

            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();

            // Clear cache after permission update
            _cacheManagement.ClearAllPermissionCaches();

            SetSuccessMessage("Permission updated successfully");
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var permission = await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission == null)
        {
            return Json(new { success = false, message = "Permission not found" });
        }

        try
        {
            // Remove all role associations
            _context.RolePermissions.RemoveRange(permission.RolePermissions);
            
            // Remove the permission
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            // Clear cache after permission deletion
            _cacheManagement.ClearAllPermissionCaches();

            return Json(new { success = true, message = "Permission deleted successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error deleting permission: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkAssign(string roleId, string area)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Json(new { success = false, message = "Role not found" });
        }

        try
        {
            // Get all permissions for the area
            var permissions = await _context.Permissions
                .Where(p => p.Area == area && p.IsActive)
                .ToListAsync();

            // Get existing permissions for this role in this area
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission.Area == area)
                .ToListAsync();

            // Remove existing permissions for this area
            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add all permissions for this area
            var newPermissions = permissions.Select(p => new RolePermission
            {
                RoleId = roleId,
                PermissionId = p.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            });

            await _context.RolePermissions.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync();

            // Clear cache after bulk assignment
            _cacheManagement.ClearAllPermissionCaches();

            return Json(new { success = true, message = $"All permissions for {area} assigned to {role.Name}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error assigning permissions: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ClearCache()
    {
        // Check if user is SuperAdmin
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsSuperAdmin)
        {
            return Json(new { success = false, message = "Only SuperAdmin can clear cache" });
        }

        try
        {
            _cacheManagement.ClearAllPermissionCaches();
            var cachedItemCount = _cacheManagement.GetCachedItemCount();
            
            // Log the cache clear action
            await _auditService.LogPermissionChangeAsync(
                "ClearCache",
                "PermissionCache",
                "All",
                user.Id,
                null,
                new { Action = "Manual cache clear by SuperAdmin", RemainingItems = cachedItemCount }
            );

            return Json(new { 
                success = true, 
                message = "Permission cache cleared successfully", 
                remainingItems = cachedItemCount 
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error clearing cache: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CacheStatus()
    {
        // Check if user is SuperAdmin
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsSuperAdmin)
        {
            return Json(new { success = false, message = "Only SuperAdmin can view cache status" });
        }

        var cachedItemCount = _cacheManagement.GetCachedItemCount();
        
        return Json(new { 
            success = true, 
            cachedItems = cachedItemCount,
            cacheExpiration = "5 minutes",
            status = "Active"
        });
    }

    public async Task<IActionResult> TestDashboard()
    {
        // Check if user is SuperAdmin
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsSuperAdmin)
        {
            return Forbid("Only SuperAdmin can access the permission testing dashboard");
        }

        // Get all users for testing
        var users = await _userManager.Users
            .Where(u => !u.IsSuperAdmin)
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email, u.UserName })
            .ToListAsync();

        // Get all permissions grouped by area
        var permissions = await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Area)
            .ThenBy(p => p.Controller)
            .ThenBy(p => p.Action)
            .ToListAsync();

        var model = new PermissionTestDashboardViewModel
        {
            Users = users.Select(u => new UserTestInfo
            {
                Id = u.Id,
                FullName = u.FullName ?? "",
                Email = u.Email ?? "",
                UserName = u.UserName ?? ""
            }).ToList(),
            Permissions = permissions.GroupBy(p => p.Area ?? "Root")
                .ToDictionary(g => g.Key, g => g.ToList())
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> TestPermission([FromBody] PermissionTestRequest request)
    {
        // Check if user is SuperAdmin
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsSuperAdmin)
        {
            return Json(new { success = false, message = "Only SuperAdmin can test permissions" });
        }

        try
        {
            // Get the test user
            var testUser = await _userManager.FindByIdAsync(request.UserId);
            if (testUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Get user's roles
            var roles = await _userManager.GetRolesAsync(testUser);

            // Create claims principal for the test user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, testUser.Id),
                new Claim(ClaimTypes.Name, testUser.UserName ?? ""),
                new Claim(ClaimTypes.Email, testUser.Email ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            // Test the permission using the permission service
            using var scope = HttpContext.RequestServices.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            
            var hasPermission = await permissionService.HasPermissionAsync(
                principal, 
                request.Area ?? "", 
                request.Controller, 
                request.Action
            );

            // Get additional info
            var isSuperAdmin = await permissionService.IsSuperAdminAsync(principal);
            var hasAreaPermission = !string.IsNullOrEmpty(request.Area) 
                ? await permissionService.UserHasAnyPermissionInAreaAsync(principal, request.Area)
                : false;

            return Json(new
            {
                success = true,
                hasPermission = hasPermission,
                isSuperAdmin = isSuperAdmin,
                hasAreaPermission = hasAreaPermission,
                userRoles = roles,
                testDetails = new
                {
                    userId = testUser.Id,
                    userName = testUser.FullName,
                    area = request.Area ?? "None",
                    controller = request.Controller,
                    action = request.Action,
                    permissionString = $"{request.Area ?? ""}/{request.Controller}/{request.Action}".Trim('/')
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error testing permission: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> TestBulkPermissions([FromBody] BulkPermissionTestRequest request)
    {
        // Check if user is SuperAdmin
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsSuperAdmin)
        {
            return Json(new { success = false, message = "Only SuperAdmin can test permissions" });
        }

        try
        {
            var results = new List<object>();

            foreach (var userId in request.UserIds)
            {
                var testUser = await _userManager.FindByIdAsync(userId);
                if (testUser == null) continue;

                var roles = await _userManager.GetRolesAsync(testUser);

                // Create claims principal
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, testUser.Id),
                    new Claim(ClaimTypes.Name, testUser.UserName ?? ""),
                    new Claim(ClaimTypes.Email, testUser.Email ?? "")
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);

                using var scope = HttpContext.RequestServices.CreateScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                
                var hasPermission = await permissionService.HasPermissionAsync(
                    principal, 
                    request.Area ?? "", 
                    request.Controller, 
                    request.Action
                );

                results.Add(new
                {
                    userId = testUser.Id,
                    userName = testUser.FullName,
                    hasPermission = hasPermission,
                    roles = roles
                });
            }

            return Json(new
            {
                success = true,
                results = results,
                testDetails = new
                {
                    area = request.Area ?? "None",
                    controller = request.Controller,
                    action = request.Action,
                    testedUsers = request.UserIds.Count
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error testing permissions: {ex.Message}" });
        }
    }

    private List<SelectListItem> GetDistinctAreas()
    {
        var areas = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "No Area (Root)" },
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "CustomerSupport", Text = "CustomerSupport" },
            new SelectListItem { Value = "Identity", Text = "Identity" }
        };

        return areas;
    }
}