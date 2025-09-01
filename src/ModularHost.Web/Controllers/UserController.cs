using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Models.Entities;
using MRCMS.Models.ViewModels;
using MRCMS.Services.Interfaces;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        
        protected virtual string EntityName => "User";
        protected virtual int PageSize => 10;

        public UserController(
            UserManager<User> userManager, 
            RoleManager<Role> roleManager,
            ILoggerService logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int page = 1, string search = null, string sortBy = null, bool sortDesc = false)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Index - Page: {Page}, Search: {Search}", EntityName, page, search);
                
                var query = _userManager.Users.AsQueryable();
                
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = ApplySearch(query, search);
                }
                
                // Apply sorting
                query = ApplySorting(query, sortBy, sortDesc);
                
                // Get total count for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
                
                // Apply pagination
                var users = await query
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var userViewModels = new List<UserViewModel>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        IsSuperAdmin = user.IsSuperAdmin,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber ?? "",
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    });
                }
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.Search = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;
                ViewBag.EntityName = EntityName;

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Index", ex, EntityName);
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation("Viewing {EntityName} Details - Id: {Id}", EntityName, id);
                
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("{EntityName} not found - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var viewModel = new UserDetailsViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    PostalCode = user.PostalCode,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    IsActive = user.IsActive,
                    IsSuperAdmin = user.IsSuperAdmin,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLoginAt = user.LastLoginAt
                };
                
                ViewBag.EntityName = EntityName;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error viewing {EntityName} Details - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Create form", EntityName);
                
                var viewModel = new CreateUserViewModel
                {
                    AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync()
                };
                
                ViewBag.EntityName = EntityName;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Create form", ex, EntityName);
                return View("Error");
            }
        }

[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Create", EntityName);
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
                    ViewBag.EntityName = EntityName;
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = model.EmailConfirmed,
                    IsActive = model.IsActive
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    
                    _logger.LogInformation("{EntityName} created successfully - Id: {Id}", EntityName, user.Id);
                    
                    TempData["SuccessMessage"] = $"{EntityName} created successfully!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error creating {EntityName}", ex, EntityName);
                ModelState.AddModelError("", "An error occurred while creating the user.");
            }

            model.AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
            ViewBag.EntityName = EntityName;
            return View(model);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Edit form - Id: {Id}", EntityName, id);
                
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("{EntityName} not found for edit - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var viewModel = new EditUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    PostalCode = user.PostalCode,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    IsActive = user.IsActive,
                    IsSuperAdmin = user.IsSuperAdmin,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    SelectedRoles = userRoles.ToList(),
                    AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync()
                };
                
                ViewBag.EntityName = EntityName;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Edit form - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }

[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();
                
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Edit - Id: {Id}", EntityName, id);
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
                    ViewBag.EntityName = EntityName;
                    return View(model);
                }

                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("{EntityName} not found for update - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.DateOfBirth = model.DateOfBirth;
                user.Address = model.Address;
                user.City = model.City;
                user.Country = model.Country;
                user.PostalCode = model.PostalCode;
                user.PhoneNumber = model.PhoneNumber;
                user.ProfilePicture = model.ProfilePicture;
                user.IsActive = model.IsActive;
                user.IsSuperAdmin = model.IsSuperAdmin;
                user.EmailConfirmed = model.EmailConfirmed;
                user.TwoFactorEnabled = model.TwoFactorEnabled;
                user.LockoutEnabled = model.LockoutEnabled;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
                    var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);
                    
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    
                    _logger.LogInformation("{EntityName} updated successfully - Id: {Id}", EntityName, id);
                    
                    TempData["SuccessMessage"] = $"{EntityName} updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error updating {EntityName} - Id: {Id}", ex, EntityName, id);
                ModelState.AddModelError("", "An error occurred while updating the user.");
            }

            model.AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
            ViewBag.EntityName = EntityName;
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Delete confirmation - Id: {Id}", EntityName, id);
                
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("{EntityName} not found for delete - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var viewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsSuperAdmin = user.IsSuperAdmin,
                    Roles = roles.ToList()
                };
                
                ViewBag.EntityName = EntityName;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Delete confirmation - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting {EntityName} - Id: {Id}", EntityName, id);
                
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("{EntityName} not found for deletion - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("{EntityName} deleted successfully - Id: {Id}", EntityName, id);
                    TempData["SuccessMessage"] = $"{EntityName} deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete {EntityName} - Id: {Id}. Errors: {Errors}", null, EntityName, id, string.Join(", ", result.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error deleting {EntityName} - Id: {Id}", ex, EntityName, id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("{EntityName} status toggled - Id: {Id}, IsActive: {IsActive}", 
                        EntityName, id, user.IsActive);
                    
                    return Json(new { success = true, isActive = user.IsActive });
                }

                return Json(new { success = false, message = "Failed to update user status." });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error toggling {EntityName} status - Id: {Id}", ex, EntityName, id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password reset successfully." });
            }

            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
        
        #region Protected Helper Methods
        
        protected virtual IQueryable<User> ApplySearch(IQueryable<User> query, string search)
        {
            return query.Where(u => 
                u.UserName.Contains(search) ||
                u.Email.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));
        }
        
        protected virtual IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, bool sortDesc)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return sortDesc 
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt);
            }
            
            return sortBy.ToLower() switch
            {
                "username" => sortDesc ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "firstname" => sortDesc ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => sortDesc ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "createdat" => sortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => sortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };
        }
        
        #endregion
    }
}