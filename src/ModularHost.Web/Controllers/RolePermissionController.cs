using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MRCMS.Core.Controllers;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Enums;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using MRCMS.Core.Infrastructure;
using MRCMS.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MRCMS.Controllers
{
    [RequireAuthentication]
    public class RolePermissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRepository<RolePermission> _repository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _logger;
        private readonly UserManager<User> _userManager;
        private readonly IPermissionService _permissionService;

        public RolePermissionController(
            AppDbContext context,
            IRepository<RolePermission> repository,
            IRepository<Role> roleRepository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IPermissionService permissionService)
        {
            _context = context;
            _repository = repository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var permissions = await _context.RolePermissions
                .Include(rp => rp.Role)
                .OrderBy(rp => rp.Role.Name)
                .ThenBy(rp => rp.Url)
                .ToListAsync();

            return View(permissions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolePermission model, List<Guid> selectedRoles)
        {
            try
            {
                if (selectedRoles?.Any() == true)
                {
                    foreach (var roleId in selectedRoles)
                    {
                        var rolePermission = new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = roleId,
                            PermissionName = model.PermissionName,
                            Url = model.Url ?? "",
                            HttpMethod = model.HttpMethod ?? "",
                            Description = model.Description ?? "",
                            AccessType = model.AccessType,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        await _repository.AddAsync(rolePermission);
                    }

                    await _unitOfWork.CommitAsync();
                    
                    // Invalidate cache for affected roles
                    foreach (var roleId in selectedRoles)
                    {
                        await _permissionService.InvalidateCacheForRoleAsync(roleId);
                    }

                    TempData["Success"] = "Permissions created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Please select at least one role.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating role permissions", ex);
                TempData["Error"] = "Failed to create permissions. Please try again.";
            }

            await PopulateViewBag();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var permission = await _repository.Query()
                .Include(rp => rp.Role)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (permission == null)
            {
                TempData["Error"] = "Permission not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewBag();
            ViewBag.SelectedRoleId = permission.RoleId;
            return View(permission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RolePermission model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Invalid permission ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existingPermission = await _repository.GetByIdAsync(id);
                if (existingPermission == null)
                {
                    TempData["Error"] = "Permission not found.";
                    return RedirectToAction(nameof(Index));
                }

                var oldRoleId = existingPermission.RoleId;

                existingPermission.RoleId = model.RoleId;
                existingPermission.PermissionName = model.PermissionName;
                existingPermission.Url = model.Url;
                existingPermission.HttpMethod = model.HttpMethod;
                existingPermission.Description = model.Description;
                existingPermission.AccessType = model.AccessType;
                existingPermission.UpdatedAt = DateTime.UtcNow;

                _repository.Update(existingPermission);
                await _unitOfWork.CommitAsync();

                // Invalidate cache for old and new roles
                await _permissionService.InvalidateCacheForRoleAsync(oldRoleId);
                if (oldRoleId != model.RoleId)
                {
                    await _permissionService.InvalidateCacheForRoleAsync(model.RoleId);
                }

                TempData["Success"] = "Permission updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating role permission", ex);
                TempData["Error"] = "Failed to update permission. Please try again.";
            }

            await PopulateViewBag();
            ViewBag.SelectedRoleId = model.RoleId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var permission = await _repository.Query()
                .Include(rp => rp.Role)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (permission == null)
            {
                TempData["Error"] = "Permission not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(permission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var permission = await _repository.GetByIdAsync(id);
                if (permission == null)
                {
                    TempData["Error"] = "Permission not found.";
                    return RedirectToAction(nameof(Index));
                }

                var roleId = permission.RoleId;
                permission.IsDeleted = true;
                _repository.Update(permission);
                await _unitOfWork.CommitAsync();

                // Invalidate cache for the role
                await _permissionService.InvalidateCacheForRoleAsync(roleId);

                TempData["Success"] = "Permission deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting role permission", ex);
                TempData["Error"] = "Failed to delete permission. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BulkAssign()
        {
            await PopulateViewBag();
            ViewBag.CommonUrls = GetCommonUrls();
            ViewBag.HttpMethods = GetHttpMethods();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssign(
            List<Guid> selectedRoles,
            List<string> selectedUrls,
            List<string> selectedMethods,
            string permissionName,
            string description)
        {
            try
            {
                if (selectedRoles?.Any() != true)
                {
                    ModelState.AddModelError("", "Please select at least one role.");
                }

                if (selectedUrls?.Any() != true)
                {
                    ModelState.AddModelError("", "Please select at least one URL.");
                }

                if (selectedMethods?.Any() != true)
                {
                    ModelState.AddModelError("", "Please select at least one HTTP method.");
                }

                if (ModelState.IsValid)
                {
                    var createdCount = 0;

                    foreach (var roleId in selectedRoles)
                    {
                        foreach (var url in selectedUrls)
                        {
                            foreach (var method in selectedMethods)
                            {
                                // Check if permission already exists
                                var exists = await _context.RolePermissions
                                    .AnyAsync(rp => rp.RoleId == roleId && rp.Url == url && rp.HttpMethod == method);

                                if (!exists)
                                {
                                    var rolePermission = new RolePermission
                                    {
                                        Id = Guid.NewGuid(),
                                        RoleId = roleId,
                                        PermissionName = permissionName ?? "",
                                        Url = url,
                                        HttpMethod = method,
                                        Description = description ?? "",
                                        AccessType = AccessType.Authorized, // Default for bulk assign
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };

                                    await _repository.AddAsync(rolePermission);
                                    createdCount++;
                                }
                            }
                        }
                    }

                    if (createdCount > 0)
                    {
                        await _unitOfWork.CommitAsync();

                        // Invalidate cache for affected roles
                        foreach (var roleId in selectedRoles)
                        {
                            await _permissionService.InvalidateCacheForRoleAsync(roleId);
                        }

                        TempData["Success"] = $"Successfully created {createdCount} permissions!";
                    }
                    else
                    {
                        TempData["Warning"] = "All selected permissions already exist.";
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in bulk assign permissions", ex);
                TempData["Error"] = "Failed to assign permissions. Please try again.";
            }

            await PopulateViewBag();
            ViewBag.CommonUrls = GetCommonUrls();
            ViewBag.HttpMethods = GetHttpMethods();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RolePermissions(Guid roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .OrderBy(rp => rp.Url)
                .ToListAsync();

            ViewBag.RoleName = role.Name;
            ViewBag.RoleId = roleId;
            return View(permissions);
        }

        private async Task PopulateViewBag()
        {
            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToListAsync();

            ViewBag.Roles = roles;
        }

        private static List<SelectListItem> GetCommonUrls()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "/admin", Text = "Admin Dashboard" },
                new SelectListItem { Value = "/user", Text = "User Management" },
                new SelectListItem { Value = "/role", Text = "Role Management" },
                new SelectListItem { Value = "/menu", Text = "Menu Management" },
                new SelectListItem { Value = "/rolepermission", Text = "Permission Management" },
                new SelectListItem { Value = "/reports", Text = "Reports" },
                new SelectListItem { Value = "/settings", Text = "System Settings" },
                new SelectListItem { Value = "/blog", Text = "Blog Management" },
                new SelectListItem { Value = "/category", Text = "Category Management" },
                new SelectListItem { Value = "/tag", Text = "Tag Management" },
                new SelectListItem { Value = "/project", Text = "Project Management" }
            };
        }

        private static List<SelectListItem> GetHttpMethods()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "GET", Text = "GET (View/Read)" },
                new SelectListItem { Value = "POST", Text = "POST (Create)" },
                new SelectListItem { Value = "PUT", Text = "PUT (Update)" },
                new SelectListItem { Value = "DELETE", Text = "DELETE (Remove)" },
                new SelectListItem { Value = "*", Text = "* (All Methods)" }
            };
        }
    }
}