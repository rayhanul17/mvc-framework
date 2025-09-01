using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MRCMS.Core.Controllers
{
    /// <summary>
    /// Simplified base CRUD controller that works directly with entities without view models
    /// Perfect for simple admin CRUD operations
    /// </summary>
    public abstract class BaseController<TEntity> : Controller
        where TEntity : BaseEntity
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILoggerService _logger;
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
        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
        protected bool IsSuperAdmin => User.IsInRole("SuperAdmin");
        protected bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Administrator") || IsSuperAdmin;
        protected bool IsInRole(string role) => User.IsInRole(role);
        
        protected virtual string EntityName => typeof(TEntity).Name;
        protected virtual string ControllerName => GetType().Name.Replace("Controller", "");
        protected virtual int PageSize => 10;

        protected BaseController(
            IRepository<TEntity> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Index(int page = 1, string search = null)
        {
            var query = _repository.Query();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = ApplySearch(query, search);
            }
            
            query = ApplyFilters(query);
            
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            
            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;
            
            return View(items);
        }

        [HttpGet("details/{id}")]
        public virtual async Task<IActionResult> Details(Guid id)
        {
            var entity = await GetEntityWithIncludes(id);
            
            if (entity == null)
                return NotFound();
            
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;
            
            return View(entity);
        }

        [HttpGet("create")]
        public virtual async Task<IActionResult> Create()
        {
            await PopulateViewBag();
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;
            
            return View(CreateNewEntity());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(TEntity entity)
        {
            try
            {
                if (!await ValidateEntity(entity))
                {
                    await PopulateViewBag();
                    ViewBag.EntityName = EntityName;
                    ViewBag.ControllerName = ControllerName;
                    return View(entity);
                }
                
                await BeforeCreate(entity);
                
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.CreatedBy = CurrentUserId;
                entity.UpdatedBy = CurrentUserId;
                
                await _repository.AddAsync(entity);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} created - Id: {Id}", EntityName, entity.Id);
                
                TempData["Success"] = $"{EntityName} created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating {EntityName}", ex, EntityName);
                ModelState.AddModelError("", "An error occurred while saving.");
                await PopulateViewBag();
                ViewBag.EntityName = EntityName;
                ViewBag.ControllerName = ControllerName;
                return View(entity);
            }
        }

        [HttpGet("edit/{id}")]
        public virtual async Task<IActionResult> Edit(Guid id)
        {
            var entity = await GetEntityWithIncludes(id);
            
            if (entity == null)
                return NotFound();
            
            await PopulateViewBag();
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;
            
            return View(entity);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(Guid id, TEntity entity)
        {
            if (id != entity.Id)
                return NotFound();
            
            try
            {
                if (!await ValidateEntity(entity, isEdit: true))
                {
                    await PopulateViewBag();
                    ViewBag.EntityName = EntityName;
                    ViewBag.ControllerName = ControllerName;
                    return View(entity);
                }
                
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    return NotFound();
                
                await BeforeUpdate(entity, existingEntity);
                
                // Preserve original creation info
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = CurrentUserId;
                
                _repository.Update(entity);
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} updated - Id: {Id}", EntityName, entity.Id);
                
                TempData["Success"] = $"{EntityName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating {EntityName} - Id: {Id}", ex, EntityName, id);
                ModelState.AddModelError("", "An error occurred while saving.");
                await PopulateViewBag();
                ViewBag.EntityName = EntityName;
                ViewBag.ControllerName = ControllerName;
                return View(entity);
            }
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                
                if (entity == null)
                    return NotFound();
                
                if (!await CanDelete(entity))
                {
                    TempData["Error"] = "This record cannot be deleted.";
                    return RedirectToAction(nameof(Index));
                }
                
                await BeforeDelete(entity);
                
                if (UseSoftDelete)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = CurrentUserId;
                    _repository.Update(entity);
                }
                else
                {
                    _repository.Remove(entity);
                }
                
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("{EntityName} deleted - Id: {Id}", EntityName, id);
                
                TempData["Success"] = $"{EntityName} deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting {EntityName} - Id: {Id}", ex, EntityName, id);
                TempData["Error"] = "An error occurred while deleting.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("toggle-status/{id}")]
        public virtual async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return Json(new { success = false });
                
                entity.IsActive = !entity.IsActive;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = CurrentUserId;
                
                _repository.Update(entity);
                await _unitOfWork.CommitAsync();
                
                return Json(new { success = true, isActive = entity.IsActive });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        #region Protected Virtual Methods - Override in derived controllers

        protected virtual bool UseSoftDelete => true;

        protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string search)
        {
            // Override to implement search
            return query;
        }

        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query)
        {
            if (UseSoftDelete)
            {
                query = query.Where(e => !e.IsDeleted);
            }
            return query;
        }

        protected virtual async Task<TEntity> GetEntityWithIncludes(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        protected virtual async Task PopulateViewBag()
        {
            await Task.CompletedTask;
        }

        protected virtual async Task<bool> ValidateEntity(TEntity entity, bool isEdit = false)
        {
            return await Task.FromResult(ModelState.IsValid);
        }

        protected virtual async Task BeforeCreate(TEntity entity)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task BeforeUpdate(TEntity entity, TEntity existingEntity)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task BeforeDelete(TEntity entity)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task<bool> CanDelete(TEntity entity)
        {
            return await Task.FromResult(true);
        }
        
        protected virtual TEntity CreateNewEntity()
        {
            return Activator.CreateInstance<TEntity>();
        }

        #endregion
    }
}