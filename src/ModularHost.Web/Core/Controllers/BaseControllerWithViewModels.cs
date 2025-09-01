using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MRCMS.Core.Controllers
{
    public abstract class BaseControllerWithViewModels<TEntity, TViewModel, TCreateViewModel, TEditViewModel> : Controller
        where TEntity : BaseEntity
        where TViewModel : class
        where TCreateViewModel : class
        where TEditViewModel : class
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILoggerService _logger;
        protected readonly IMapper _mapper;
        protected readonly UserManager<User> _userManager;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        
        private User _currentUser;
        protected User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
                    {
                        _currentUser = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
                    }
                }
                return _currentUser;
            }
        }
        
        protected Guid? CurrentUserId => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
        protected string CurrentUserName => User.Identity?.Name ?? "Anonymous";
        protected string CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value ?? CurrentUser?.Email ?? "";
        protected string CurrentUserFullName => CurrentUser != null ? $"{CurrentUser.FirstName} {CurrentUser.LastName}" : "Anonymous";
        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
        protected bool IsSuperAdmin => User.HasClaim("IsSuperAdmin", "true");
        protected bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Administrator") || IsSuperAdmin;
        protected bool IsInRole(string role) => User.IsInRole(role);
        
        protected virtual string EntityName => typeof(TEntity).Name.Replace("Entity", "");
        protected virtual string ViewPrefix => "";
        protected virtual int PageSize => 10;

        protected BaseControllerWithViewModels(
            IRepository<TEntity> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            IMapper mapper,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Index/List
        
        [HttpGet]
        public virtual async Task<IActionResult> Index(int page = 1, string search = null, string sortBy = null, bool sortDesc = false)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Index - Page: {Page}, Search: {Search}", EntityName, page, search);
                
                var query = _repository.Query();
                
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = ApplySearch(query, search);
                }
                
                // Apply custom filters
                query = ApplyCustomFilters(query);
                
                // Apply sorting
                query = ApplySorting(query, sortBy, sortDesc);
                
                // Get total count for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
                
                // Apply pagination
                var items = await query
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
                
                var viewModels = _mapper.Map<List<TViewModel>>(items);
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.Search = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;
                ViewBag.EntityName = EntityName;
                
                return View($"{ViewPrefix}Index", viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Index", ex, EntityName);
                return View("Error");
            }
        }
        
        #endregion

        #region Details
        
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation("Viewing {EntityName} Details - Id: {Id}", EntityName, id);
                
                var entity = await GetEntityWithIncludes(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("{EntityName} not found - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                var viewModel = _mapper.Map<TViewModel>(entity);
                
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Details", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error viewing {EntityName} Details - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }
        
        #endregion

        #region Create
        
        [HttpGet("create")]
        public virtual async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Create form", EntityName);
                
                var viewModel = CreateNewViewModel();
                await PopulateViewBagForCreate();
                
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Create", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Create form", ex, EntityName);
                return View("Error");
            }
        }
        
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(TCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Create", EntityName);
                    await PopulateViewBagForCreate();
                    ViewBag.EntityName = EntityName;
                    return View($"{ViewPrefix}Create", model);
                }
                
                // Validate business rules
                var validationResult = await ValidateCreateModel(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Message);
                    }
                    await PopulateViewBagForCreate();
                    ViewBag.EntityName = EntityName;
                    return View($"{ViewPrefix}Create", model);
                }
                
                var entity = _mapper.Map<TEntity>(model);
                
                // Apply any custom entity configuration before save
                await ConfigureEntityForCreate(entity, model);
                
                await _repository.AddAsync(entity);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} created successfully - Id: {Id}", EntityName, entity.Id);
                
                TempData["SuccessMessage"] = $"{EntityName} created successfully!";
                
                return RedirectToAction(nameof(Details), new { id = entity.Id });
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error creating {EntityName}", ex, EntityName);
                ModelState.AddModelError("", "An error occurred while creating the record.");
                await PopulateViewBagForCreate();
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Create", model);
            }
        }
        
        #endregion

        #region Edit
        
        [HttpGet("edit/{id}")]
        public virtual async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Edit form - Id: {Id}", EntityName, id);
                
                var entity = await GetEntityWithIncludes(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("{EntityName} not found for edit - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                if (!await CanEditEntity(entity))
                {
                    _logger.LogWarning("User not authorized to edit {EntityName} - Id: {Id}", EntityName, id);
                    return Forbid();
                }
                
                var viewModel = _mapper.Map<TEditViewModel>(entity);
                await PopulateViewBagForEdit(entity);
                
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Edit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Edit form - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }
        
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(Guid id, TEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for {EntityName} Edit - Id: {Id}", EntityName, id);
                    await PopulateViewBagForEdit(null);
                    ViewBag.EntityName = EntityName;
                    return View($"{ViewPrefix}Edit", model);
                }
                
                var entity = await GetEntityWithIncludes(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("{EntityName} not found for update - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                if (!await CanEditEntity(entity))
                {
                    _logger.LogWarning("User not authorized to update {EntityName} - Id: {Id}", EntityName, id);
                    return Forbid();
                }
                
                // Validate business rules
                var validationResult = await ValidateEditModel(model, entity);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Message);
                    }
                    await PopulateViewBagForEdit(entity);
                    ViewBag.EntityName = EntityName;
                    return View($"{ViewPrefix}Edit", model);
                }
                
                // Map changes to entity
                _mapper.Map(model, entity);
                
                // Apply any custom entity configuration before save
                await ConfigureEntityForEdit(entity, model);
                
                _repository.Update(entity);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} updated successfully - Id: {Id}", EntityName, id);
                
                TempData["SuccessMessage"] = $"{EntityName} updated successfully!";
                
                return RedirectToAction(nameof(Details), new { id = entity.Id });
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error updating {EntityName} - Id: {Id}", ex, EntityName, id);
                ModelState.AddModelError("", "An error occurred while updating the record.");
                await PopulateViewBagForEdit(null);
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Edit", model);
            }
        }
        
        #endregion

        #region Delete
        
        [HttpGet("delete/{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Accessing {EntityName} Delete confirmation - Id: {Id}", EntityName, id);
                
                var entity = await GetEntityWithIncludes(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("{EntityName} not found for delete - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                if (!await CanDeleteEntity(entity))
                {
                    _logger.LogWarning("User not authorized to delete {EntityName} - Id: {Id}", EntityName, id);
                    return Forbid();
                }
                
                var viewModel = _mapper.Map<TViewModel>(entity);
                
                ViewBag.EntityName = EntityName;
                return View($"{ViewPrefix}Delete", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error accessing {EntityName} Delete confirmation - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }
        
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting {EntityName} - Id: {Id}", EntityName, id);
                
                var entity = await _repository.GetByIdAsync(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("{EntityName} not found for deletion - Id: {Id}", EntityName, id);
                    return NotFound();
                }
                
                if (!await CanDeleteEntity(entity))
                {
                    _logger.LogWarning("User not authorized to delete {EntityName} - Id: {Id}", EntityName, id);
                    return Forbid();
                }
                
                // Check if entity can be deleted (e.g., no dependent records)
                var canDelete = await CanDeleteEntityBusinessRule(entity);
                if (!canDelete.CanDelete)
                {
                    TempData["ErrorMessage"] = canDelete.Reason;
                    return RedirectToAction(nameof(Details), new { id = entity.Id });
                }
                
                // Perform soft delete or hard delete based on configuration
                if (UseSoftDelete)
                {
                    entity.IsDeleted = true;
                    _repository.Update(entity);
                }
                else
                {
                    _repository.Remove(entity);
                }
                
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} deleted successfully - Id: {Id}", EntityName, id);
                
                TempData["SuccessMessage"] = $"{EntityName} deleted successfully!";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalWithEmailAsync("Error deleting {EntityName} - Id: {Id}", ex, EntityName, id);
                TempData["ErrorMessage"] = "An error occurred while deleting the record.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
        
        #endregion

        #region Protected Virtual Methods - Override these in derived controllers
        
        protected virtual bool UseSoftDelete => true;
        
        protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string search)
        {
            // Override in derived class to implement search logic
            return query;
        }
        
        protected virtual IQueryable<TEntity> ApplyCustomFilters(IQueryable<TEntity> query)
        {
            // Filter out soft-deleted items by default
            if (UseSoftDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }
            return query;
        }
        
        protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sortBy, bool sortDesc)
        {
            // Default sorting by CreatedAt
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return sortDesc 
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt);
            }
            
            // Override in derived class for custom sorting
            return query;
        }
        
        protected virtual async Task<TEntity> GetEntityWithIncludes(Guid id)
        {
            // Override to include related entities
            return await _repository.GetByIdAsync(id);
        }
        
        protected virtual TCreateViewModel CreateNewViewModel()
        {
            return Activator.CreateInstance<TCreateViewModel>();
        }
        
        protected virtual async Task PopulateViewBagForCreate()
        {
            // Override to populate dropdowns, etc.
            await Task.CompletedTask;
        }
        
        protected virtual async Task PopulateViewBagForEdit(TEntity entity)
        {
            // Override to populate dropdowns, etc.
            await PopulateViewBagForCreate();
        }
        
        protected virtual async Task<ValidationResult> ValidateCreateModel(TCreateViewModel model)
        {
            // Override for custom validation
            return await Task.FromResult(new ValidationResult { IsValid = true });
        }
        
        protected virtual async Task<ValidationResult> ValidateEditModel(TEditViewModel model, TEntity entity)
        {
            // Override for custom validation
            return await Task.FromResult(new ValidationResult { IsValid = true });
        }
        
        protected virtual async Task ConfigureEntityForCreate(TEntity entity, TCreateViewModel model)
        {
            // Set user tracking fields
            entity.CreatedBy = CurrentUserId;
            entity.UpdatedBy = CurrentUserId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Override to set additional properties
            await Task.CompletedTask;
        }
        
        protected virtual async Task ConfigureEntityForEdit(TEntity entity, TEditViewModel model)
        {
            // Set user tracking fields
            entity.UpdatedBy = CurrentUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Override to set additional properties
            await Task.CompletedTask;
        }
        
        protected virtual async Task<bool> CanEditEntity(TEntity entity)
        {
            // Override for custom authorization logic
            return await Task.FromResult(true);
        }
        
        protected virtual async Task<bool> CanDeleteEntity(TEntity entity)
        {
            // Override for custom authorization logic
            return await Task.FromResult(true);
        }
        
        protected virtual async Task<DeleteValidationResult> CanDeleteEntityBusinessRule(TEntity entity)
        {
            // Override to check for dependent records, etc.
            return await Task.FromResult(new DeleteValidationResult { CanDelete = true });
        }
        
        #endregion

        #region Helper Classes
        
        protected class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        }
        
        protected class ValidationError
        {
            public string Key { get; set; } = "";
            public string Message { get; set; } = "";
        }
        
        protected class DeleteValidationResult
        {
            public bool CanDelete { get; set; }
            public string Reason { get; set; }
        }
        
        #endregion

        #region AJAX/API Methods
        
        [HttpPost("toggle-status/{id}")]
        public virtual async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return Json(new { success = false, message = "Record not found" });
                
                if (!await CanEditEntity(entity))
                    return Json(new { success = false, message = "Not authorized" });
                
                entity.IsActive = !entity.IsActive;
                _repository.Update(entity);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} status toggled - Id: {Id}, IsActive: {IsActive}", 
                    EntityName, id, entity.IsActive);
                
                return Json(new { success = true, isActive = entity.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error toggling {EntityName} status - Id: {Id}", ex, EntityName, id);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
        
        [HttpGet("export")]
        public virtual async Task<IActionResult> Export(string format = "csv")
        {
            try
            {
                _logger.LogInformation("Exporting {EntityName} data - Format: {Format}", EntityName, format);
                
                var query = _repository.Query();
                query = ApplyCustomFilters(query);
                var items = await query.ToListAsync();
                
                switch (format.ToLower())
                {
                    case "csv":
                        return await ExportToCsv(items);
                    case "excel":
                        return await ExportToExcel(items);
                    case "pdf":
                        return await ExportToPdf(items);
                    default:
                        return BadRequest("Invalid export format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error exporting {EntityName} data", ex, EntityName);
                return BadRequest("Export failed");
            }
        }
        
        protected virtual async Task<IActionResult> ExportToCsv(List<TEntity> items)
        {
            // Override in derived class to implement CSV export
            return await Task.FromResult(BadRequest("CSV export not implemented"));
        }
        
        protected virtual async Task<IActionResult> ExportToExcel(List<TEntity> items)
        {
            // Override in derived class to implement Excel export
            return await Task.FromResult(BadRequest("Excel export not implemented"));
        }
        
        protected virtual async Task<IActionResult> ExportToPdf(List<TEntity> items)
        {
            // Override in derived class to implement PDF export
            return await Task.FromResult(BadRequest("PDF export not implemented"));
        }
        
        #endregion
    }
}