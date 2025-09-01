using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Core.Infrastructure;
using MRCMS.ViewModels;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ModuleController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;

        public ModuleController(IServiceProvider serviceProvider, AppDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }

        public IActionResult Index()
        {
            var modules = _serviceProvider.GetServices<IModuleDescriptor>()
                .Select(m => new ModuleViewModel
                {
                    Name = m.Name,
                    DisplayName = m.DisplayName,
                    Description = m.Description,
                    Version = m.Version,
                    Author = m.Author,
                    TablePrefix = m.TablePrefix,
                    IsActive = m.IsActive,
                    EntityCount = m.EntityTypes?.Length ?? 0,
                    Tables = m.EntityTypes?.Select(t => t.Name).ToArray() ?? new string[0]
                })
                .OrderBy(m => m.DisplayName)
                .ToList();

            return View(modules);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleModule(string moduleName, bool isActive)
        {
            try
            {
                var module = _serviceProvider.GetServices<IModuleDescriptor>()
                    .FirstOrDefault(m => m.Name == moduleName);

                if (module == null)
                {
                    return Json(new { success = false, message = "Module not found" });
                }

                module.IsActive = isActive;
                
                // Save module state to database or configuration
                // This would typically be stored in a settings table
                
                return Json(new { success = true, message = $"Module {(isActive ? "activated" : "deactivated")} successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTables(string moduleName)
        {
            try
            {
                var module = _serviceProvider.GetServices<IModuleDescriptor>()
                    .FirstOrDefault(m => m.Name == moduleName);

                if (module == null)
                {
                    return Json(new { success = false, message = "Module not found" });
                }

                // Apply pending migrations for the module
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                    return Json(new { 
                        success = true, 
                        message = $"Applied {pendingMigrations.Count()} migration(s) for {module.DisplayName}",
                        migrationsApplied = pendingMigrations.Count()
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        message = "No pending migrations found",
                        migrationsApplied = 0
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Migration failed: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyAllMigrations()
        {
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                    return Json(new { 
                        success = true, 
                        message = $"Applied {pendingMigrations.Count()} migration(s) successfully",
                        migrationsApplied = pendingMigrations.Count()
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        message = "No pending migrations found",
                        migrationsApplied = 0
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Migration failed: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(string moduleName)
        {
            var module = _serviceProvider.GetServices<IModuleDescriptor>()
                .FirstOrDefault(m => m.Name == moduleName);

            if (module == null)
            {
                return NotFound();
            }

            var viewModel = new ModuleDetailsViewModel
            {
                Name = module.Name,
                DisplayName = module.DisplayName,
                Description = module.Description,
                Version = module.Version,
                Author = module.Author,
                TablePrefix = module.TablePrefix,
                IsActive = module.IsActive,
                Tables = module.EntityTypes?.Select(t => new TableInfo
                {
                    EntityName = t.Name,
                    TableName = t.Name.ToLower(), // This would use your extension method
                    FullTableName = $"{module.TablePrefix}_{t.Name.ToLower()}"
                }).ToList() ?? new List<TableInfo>()
            };

            // Get migration history
            var migrations = await _context.Database.GetAppliedMigrationsAsync();
            viewModel.AppliedMigrations = migrations.ToList();
            
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            viewModel.PendingMigrations = pendingMigrations.ToList();

            return View(viewModel);
        }
    }
}