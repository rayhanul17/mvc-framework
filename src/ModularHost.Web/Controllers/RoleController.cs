using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Services.Interfaces;
using MRCMS.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IAuditLogger _auditLogger;

        public RoleController(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            AppDbContext context,
            IAuditLogger auditLogger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _auditLogger = auditLogger;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Description = r.Description,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.Id),
                    PermissionCount = _context.RolePermissions.Count(rp => rp.RoleId == r.Id)
                })
                .ToListAsync();

            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new Role
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    await _auditLogger.LogAsync("Role", "Create", $"Created role: {role.Name}");
                    TempData["Success"] = $"Role '{role.Name}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description,
                IsActive = role.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id.ToString());
                if (role == null)
                {
                    return NotFound();
                }

                // Prevent editing system roles
                if (role.Name == "SuperAdmin" || role.Name == "Admin" || role.Name == "User")
                {
                    TempData["Error"] = "System roles cannot be modified.";
                    return RedirectToAction(nameof(Index));
                }

                role.Name = model.Name;
                role.Description = model.Description;
                role.IsActive = model.IsActive;
                role.UpdatedAt = DateTime.UtcNow;

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    await _auditLogger.LogAsync("Role", "Update", $"Updated role: {role.Name}");
                    TempData["Success"] = $"Role '{role.Name}' updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deleting system roles
            if (role.Name == "SuperAdmin" || role.Name == "Admin" || role.Name == "User")
            {
                TempData["Error"] = "System roles cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Check if role has users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
            if (usersInRole.Any())
            {
                TempData["Error"] = $"Cannot delete role '{role.Name}' because it has {usersInRole.Count} user(s) assigned.";
                return RedirectToAction(nameof(Index));
            }

            // Delete associated permissions
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(permissions);

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                await _auditLogger.LogAsync("Role", "Delete", $"Deleted role: {role.Name}");
                TempData["Success"] = $"Role '{role.Name}' deleted successfully.";
            }
            else
            {
                TempData["Error"] = $"Failed to delete role '{role.Name}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManagePermissions(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync();

            // Get all available menu items for permission assignment
            var menus = await _context.Menus
                .OrderBy(m => m.Order)
                .ToListAsync();

            var model = new ManagePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                Permissions = permissions,
                AvailableMenus = menus
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPermission(Guid roleId, string url, string httpMethod, string description)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Check if permission already exists
            var existingPermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && 
                                          rp.Url == url && 
                                          rp.HttpMethod == httpMethod);

            if (existingPermission != null)
            {
                TempData["Error"] = "This permission already exists for the role.";
                return RedirectToAction(nameof(ManagePermissions), new { id = roleId });
            }

            var permission = new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                Url = url,
                HttpMethod = httpMethod,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RolePermissions.Add(permission);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Permission", "Create", 
                $"Added permission {url} ({httpMethod}) to role {role.Name}");

            TempData["Success"] = "Permission added successfully.";
            return RedirectToAction(nameof(ManagePermissions), new { id = roleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePermission(Guid permissionId, Guid roleId)
        {
            var permission = await _context.RolePermissions.FindAsync(permissionId);
            if (permission != null)
            {
                _context.RolePermissions.Remove(permission);
                await _context.SaveChangesAsync();

                await _auditLogger.LogAsync("Permission", "Delete", 
                    $"Removed permission {permission.Url} ({permission.HttpMethod})");

                TempData["Success"] = "Permission removed successfully.";
            }

            return RedirectToAction(nameof(ManagePermissions), new { id = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
            var allUsers = await _userManager.Users.ToListAsync();
            var usersNotInRole = allUsers.Except(usersInRole).ToList();

            var model = new ManageRoleUsersViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                UsersInRole = usersInRole.Select(u => new UserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsActive = u.IsActive
                }).ToList(),
                UsersNotInRole = usersNotInRole.Select(u => new UserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsActive = u.IsActive
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToRole(Guid roleId, Guid userId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (role == null || user == null)
            {
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name ?? "");

            if (result.Succeeded)
            {
                await _auditLogger.LogAsync("Role", "AddUser", 
                    $"Added user {user.UserName} to role {role.Name}");
                TempData["Success"] = $"User '{user.UserName}' added to role successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to add user to role.";
            }

            return RedirectToAction(nameof(ManageUsers), new { id = roleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(Guid roleId, Guid userId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (role == null || user == null)
            {
                return NotFound();
            }

            // Prevent removing last SuperAdmin
            if (role.Name == "SuperAdmin")
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (superAdmins.Count <= 1)
                {
                    TempData["Error"] = "Cannot remove the last SuperAdmin from the system.";
                    return RedirectToAction(nameof(ManageUsers), new { id = roleId });
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name ?? "");

            if (result.Succeeded)
            {
                await _auditLogger.LogAsync("Role", "RemoveUser", 
                    $"Removed user {user.UserName} from role {role.Name}");
                TempData["Success"] = $"User '{user.UserName}' removed from role successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to remove user from role.";
            }

            return RedirectToAction(nameof(ManageUsers), new { id = roleId });
        }
    }
}