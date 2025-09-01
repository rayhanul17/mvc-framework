using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

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
        protected readonly IMemoryCache _cache;
        protected readonly IPermissionService _permissionService;
        
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
        protected bool IsSuperAdmin => User.HasClaim("IsSuperAdmin", "true");
        protected bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Administrator") || IsSuperAdmin;
        protected bool IsInRole(string role) => User.IsInRole(role);
        
        // Session helper properties
        protected string SessionId => HttpContext.Session.Id;
        protected bool HasSession => HttpContext.Session.IsAvailable;
        
        // Request helper properties
        protected string BaseUrl => $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}";
        protected string CurrentUrl => $"{BaseUrl}{Request.Path}{Request.QueryString}";
        protected string ReturnUrl => Request.Query["returnUrl"].FirstOrDefault() ?? "/";
        protected string IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        protected bool IsAjaxRequest => Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        
        protected virtual string EntityName => typeof(TEntity).Name;
        protected virtual string ControllerName => GetType().Name.Replace("Controller", "");
        protected virtual int PageSize => 10;

        protected BaseController(
            IRepository<TEntity> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache = null,
            IPermissionService permissionService = null)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _permissionService = permissionService;
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

        #region Message Helper Methods
        
        public enum MessageType
        {
            Success,
            Info,
            Warning,
            Error
        }
        
        protected void ShowMessage(string message, MessageType messageType, bool showAfterRedirect = false, int durationSeconds = 5)
        {
            var key = messageType switch
            {
                MessageType.Success => "Success",
                MessageType.Info => "Info",
                MessageType.Warning => "Warning",
                MessageType.Error => "Error",
                _ => "Info"
            };
            
            if (showAfterRedirect)
            {
                TempData[key] = message;
                TempData["MessageDuration"] = durationSeconds;
            }
            else
            {
                ViewBag.Message = message;
                ViewBag.MessageType = key;
                ViewBag.MessageDuration = durationSeconds;
            }
        }
        
        protected void ShowSuccess(string message, bool showAfterRedirect = false) => ShowMessage(message, MessageType.Success, showAfterRedirect);
        protected void ShowError(string message, bool showAfterRedirect = false) => ShowMessage(message, MessageType.Error, showAfterRedirect);
        protected void ShowWarning(string message, bool showAfterRedirect = false) => ShowMessage(message, MessageType.Warning, showAfterRedirect);
        protected void ShowInfo(string message, bool showAfterRedirect = false) => ShowMessage(message, MessageType.Info, showAfterRedirect);
        
        #endregion
        
        #region Session Helper Methods
        
        protected T GetSessionValue<T>(string key)
        {
            if (HttpContext.Session.TryGetValue(key, out var bytes))
            {
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            return default(T);
        }
        
        protected void SetSessionValue<T>(string key, T value)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            HttpContext.Session.Set(key, bytes);
        }
        
        protected void RemoveSessionValue(string key)
        {
            HttpContext.Session.Remove(key);
        }
        
        protected void ClearSession()
        {
            HttpContext.Session.Clear();
        }
        
        #endregion
        
        #region Cache Helper Methods
        
        protected T GetCache<T>(string key)
        {
            if (_cache != null && _cache.TryGetValue(key, out T value))
            {
                return value;
            }
            return default(T);
        }
        
        protected void SetCache<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (_cache != null)
            {
                var options = new MemoryCacheEntryOptions();
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    options.SlidingExpiration = TimeSpan.FromMinutes(30);
                }
                _cache.Set(key, value, options);
            }
        }
        
        protected void RemoveCache(string key)
        {
            _cache?.Remove(key);
        }
        
        #endregion
        
        #region Permission Helper Methods
        
        protected async Task<bool> HasPermissionAsync(string url, string httpMethod = "GET")
        {
            if (IsSuperAdmin) return true;
            if (_permissionService == null) return false;
            return await _permissionService.IsUrlAllowedAsync(User, url, httpMethod);
        }
        
        protected async Task<List<Menu>> GetUserMenusAsync()
        {
            if (_permissionService == null || CurrentUserId == null) return new List<Menu>();
            var menus = await _permissionService.GetMenuForUserAsync(CurrentUserId.Value);
            return menus?.ToList() ?? new List<Menu>();
        }
        
        protected bool CheckSuperAdmin()
        {
            return IsSuperAdmin;
        }
        
        #endregion
        
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