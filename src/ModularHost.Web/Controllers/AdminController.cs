using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using MRCMS.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IAuditLogger _auditLogger;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<RolePermission> _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _logger;
        
        protected virtual string EntityName => "Permission";
        protected virtual int PageSize => 10;

        public AdminController(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IAuditLogger auditLogger,
            IPermissionService permissionService,
            IRepository<RolePermission> permissionRepository,
            IUnitOfWork unitOfWork,
            ILoggerService logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLogger = auditLogger;
            _permissionService = permissionService;
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Accessing Admin Dashboard");
                
                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = await _userManager.Users.CountAsync(),
                    ActiveUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync(),
                    TotalRoles = await _roleManager.Roles.CountAsync(),
                    TotalMenus = await _context.Menus.CountAsync(),
                    TotalPermissions = await _context.RolePermissions.CountAsync(),
                    RecentUsers = await _userManager.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(5)
                        .Select(u => new UserSummary
                        {
                            Id = u.Id,
                            UserName = u.UserName ?? "",
                            Email = u.Email ?? "",
                            FullName = $"{u.FirstName} {u.LastName}",
                            CreatedAt = u.CreatedAt,
                            IsActive = u.IsActive
                        })
                        .ToListAsync(),
                    RecentAuditLogs = await GetRecentAuditLogsAsync()
                };

                // Get system stats
                viewModel.SystemStats = new SystemStats
                {
                    DatabaseSize = await GetDatabaseSizeAsync(),
                    CacheSize = 0, // Would need to implement cache size calculation
                    LogSize = await GetLogSizeAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing Admin Dashboard", ex);
                return View("Error");
            }
        }

        public async Task<IActionResult> Permissions(int page = 1, string search = null)
        {
            try
            {
                _logger.LogInformation("Accessing Permissions Index - Page: {Page}, Search: {Search}", page, search);
                
                IQueryable<RolePermission> query = _permissionRepository.Query().Include(p => p.Role);
                
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = ApplyPermissionSearch(query, search);
                }
                
                // Get total count for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
                
                // Apply pagination and sorting
                var permissions = await query
                    .OrderBy(p => p.Role.Name)
                    .ThenBy(p => p.Url)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
                
                // Initialize Roles list for each permission if null
                foreach (var permission in permissions)
                {
                    if (permission.Roles == null)
                    {
                        permission.Roles = new List<string>();
                    }
                    // Add the role name to the Roles list
                    if (permission.Role != null && !string.IsNullOrEmpty(permission.Role.Name))
                    {
                        permission.Roles.Add(permission.Role.Name);
                    }
                }
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.Search = search;
                ViewBag.EntityName = EntityName;

                // Return an empty list if permissions is null
                return View(permissions ?? new List<RolePermission>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing Permissions Index", ex);
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreatePermission()
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Create form", EntityName);
                
                ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                ViewBag.EntityName = EntityName;
                
                return View(new RolePermission 
                { 
                    Url = "", 
                    HttpMethod = "GET", 
                    Description = "" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Create form", ex, EntityName);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePermission(RolePermission permission)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Create", EntityName);
                    ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                    ViewBag.EntityName = EntityName;
                    return View(permission);
                }

                permission.Id = Guid.NewGuid();
                permission.CreatedAt = DateTime.UtcNow;
                permission.UpdatedAt = DateTime.UtcNow;

                await _permissionRepository.AddAsync(permission);
                await _unitOfWork.CommitAsync();

                await _auditLogger.LogAsync("Permission", "Create", 
                    $"Created permission: {permission.Url} for role {permission.RoleId}");
                
                _logger.LogInformation("{EntityName} created successfully - Id: {Id}", EntityName, permission.Id);

                TempData["SuccessMessage"] = $"{EntityName} created successfully!";
                return RedirectToAction(nameof(Permissions));
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error creating {EntityName}", ex, EntityName);
                ModelState.AddModelError("", "An error occurred while creating the permission.");
            }

            ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.EntityName = EntityName;
            return View(permission);
        }

        [HttpGet]
        public async Task<IActionResult> EditPermission(Guid id)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Edit form - Id: {Id}", EntityName, id);
                
                var permission = await _permissionRepository.GetByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("{EntityName} not found for edit - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                ViewBag.EntityName = EntityName;
                return View(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Edit form - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPermission(Guid id, RolePermission permission)
        {
            if (id != permission.Id)
                return NotFound();
                
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Edit - Id: {Id}", EntityName, id);
                    ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                    ViewBag.EntityName = EntityName;
                    return View(permission);
                }

                var existingPermission = await _permissionRepository.GetByIdAsync(id);
                if (existingPermission == null)
                {
                    _logger.LogWarning("{EntityName} not found for update - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                // Preserve original creation info
                permission.CreatedAt = existingPermission.CreatedAt;
                permission.CreatedBy = existingPermission.CreatedBy;
                permission.UpdatedAt = DateTime.UtcNow;
                
                _permissionRepository.Update(permission);
                await _unitOfWork.CommitAsync();

                await _auditLogger.LogAsync("Permission", "Update", 
                    $"Updated permission: {permission.Url}");
                    
                _logger.LogInformation("{EntityName} updated successfully - Id: {Id}", EntityName, id);

                TempData["SuccessMessage"] = $"{EntityName} updated successfully!";
                return RedirectToAction(nameof(Permissions));
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error updating {EntityName} - Id: {Id}", ex, EntityName, id);
                ModelState.AddModelError("", "An error occurred while updating the permission.");
            }

            ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.EntityName = EntityName;
            return View(permission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting {EntityName} - Id: {Id}", EntityName, id);
                
                var permission = await _permissionRepository.GetByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("{EntityName} not found for deletion - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                _permissionRepository.Remove(permission);
                await _unitOfWork.CommitAsync();

                await _auditLogger.LogAsync("Permission", "Delete", 
                    $"Deleted permission: {permission.Url}");
                    
                _logger.LogInformation("{EntityName} deleted successfully - Id: {Id}", EntityName, id);

                TempData["SuccessMessage"] = $"{EntityName} deleted successfully!";
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error deleting {EntityName} - Id: {Id}", ex, EntityName, id);
                TempData["ErrorMessage"] = "An error occurred while deleting the permission.";
            }

            return RedirectToAction(nameof(Permissions));
        }

        public async Task<IActionResult> AuditLog(string? entityName = null, string? action = null, 
            DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(a => a.EntityName == entityName);
            
            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);
            
            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value.AddDays(1));

            var totalCount = await query.CountAsync();
            
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogViewModel
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    VersionNumber = a.VersionNumber,
                    UserName = a.UserName ?? "System",
                    UserId = a.UserId,
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress,
                    ChangedProperties = a.ChangedProperties,
                    HasChanges = !string.IsNullOrEmpty(a.OldValues) || !string.IsNullOrEmpty(a.NewValues)
                })
                .ToListAsync();

            // Get unique entity names for filter dropdown
            ViewBag.EntityNames = await _context.AuditLogs
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.EntityNameFilter = entityName;
            ViewBag.ActionFilter = action;
            ViewBag.StartDateFilter = startDate;
            ViewBag.EndDateFilter = endDate;
            
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditDiff(Guid id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return NotFound();
            }

            // Get previous version
            var previousAudit = await _context.AuditLogs
                .Where(a => a.EntityName == auditLog.EntityName && 
                           a.EntityId == auditLog.EntityId &&
                           a.VersionNumber == auditLog.VersionNumber - 1)
                .FirstOrDefaultAsync();

            var diffViewModel = new AuditDiffViewModel
            {
                CurrentVersion = new AuditVersionInfo
                {
                    Id = auditLog.Id,
                    VersionNumber = auditLog.VersionNumber,
                    Action = auditLog.Action,
                    Timestamp = auditLog.Timestamp,
                    UserName = auditLog.UserName ?? "System",
                    OldValues = auditLog.OldValues,
                    NewValues = auditLog.NewValues,
                    ChangedProperties = auditLog.ChangedProperties?.Split(',').ToList() ?? new List<string>()
                }
            };

            if (previousAudit != null)
            {
                diffViewModel.PreviousVersion = new AuditVersionInfo
                {
                    Id = previousAudit.Id,
                    VersionNumber = previousAudit.VersionNumber,
                    Action = previousAudit.Action,
                    Timestamp = previousAudit.Timestamp,
                    UserName = previousAudit.UserName ?? "System",
                    OldValues = previousAudit.OldValues,
                    NewValues = previousAudit.NewValues,
                    ChangedProperties = previousAudit.ChangedProperties?.Split(',').ToList() ?? new List<string>()
                };
            }

            return Json(diffViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetEntityHistory(string entityName, Guid entityId)
        {
            var history = await _context.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.VersionNumber)
                .Select(a => new AuditHistoryItem
                {
                    Id = a.Id,
                    VersionNumber = a.VersionNumber,
                    Action = a.Action,
                    Timestamp = a.Timestamp,
                    UserName = a.UserName ?? "System",
                    ChangedProperties = a.ChangedProperties
                })
                .ToListAsync();

            return Json(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAuditLog(int daysToKeep = 30)
        {
            if (User.HasClaim("IsSuperAdmin", "true"))
            {
                await _auditLogger.ClearOldLogsAsync(daysToKeep);
                TempData["Success"] = $"Audit logs older than {daysToKeep} days have been cleared.";
            }
            else
            {
                TempData["Error"] = "Only Super Admins can clear audit logs.";
            }

            return RedirectToAction(nameof(AuditLog));
        }

        private async Task<bool> PermissionExists(Guid id)
        {
            return await _permissionRepository.Query().AnyAsync(e => e.Id == id);
        }
        
        protected virtual IQueryable<RolePermission> ApplyPermissionSearch(IQueryable<RolePermission> query, string search)
        {
            return query.Where(p => 
                p.Url.Contains(search) ||
                p.HttpMethod.Contains(search) ||
                p.Description.Contains(search) ||
                p.Role.Name.Contains(search));
        }

        private async Task<List<AuditLogEntry>> GetRecentAuditLogsAsync()
        {
            return await _auditLogger.GetLogsAsync(1, 10);
        }

        private async Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        SUM(data_length + index_length) as size
                    FROM information_schema.TABLES 
                    WHERE table_schema = DATABASE()";
                
                var result = await command.ExecuteScalarAsync();
                await connection.CloseAsync();
                
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<long> GetLogSizeAsync()
        {
            try
            {
                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (Directory.Exists(logsDirectory))
                {
                    var directoryInfo = new DirectoryInfo(logsDirectory);
                    return await Task.Run(() => directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                        .Sum(file => file.Length));
                }
            }
            catch
            {
                // Ignore errors
            }
            return 0;
        }
        
        [HttpPost]
        public async Task<IActionResult> TogglePermissionStatus(Guid id)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(id);
                if (permission == null)
                    return Json(new { success = false, message = "Permission not found" });

                permission.IsActive = !permission.IsActive;
                permission.UpdatedAt = DateTime.UtcNow;
                
                _permissionRepository.Update(permission);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} status toggled - Id: {Id}, IsActive: {IsActive}", 
                    EntityName, id, permission.IsActive);
                
                return Json(new { success = true, isActive = permission.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error toggling {EntityName} status - Id: {Id}", ex, EntityName, id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }
}