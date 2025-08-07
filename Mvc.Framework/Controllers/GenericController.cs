using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mvc.framework.Services;

namespace mvc.framework.Controllers
{
    public abstract class GenericController<T> : Controller where T : class
    {
        protected readonly IGenericService<T> _service;
        protected readonly ILogger<GenericController<T>> _logger;

        public GenericController(IGenericService<T> service, ILogger<GenericController<T>> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _service.GetPagedAsync(pageNumber, pageSize);
                
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {typeof(T).Name} index");
                TempData["Error"] = "An error occurred while loading the data.";
                return View();
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }
                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {typeof(T).Name} details for id: {id}");
                TempData["Error"] = "An error occurred while loading the details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> CreateOrEdit(int? id)
        {
            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var entity = await _service.GetByIdAsync(id.Value);
                    if (entity == null)
                    {
                        return NotFound();
                    }
                    ViewBag.IsEdit = true;
                    ViewBag.ActionTitle = "Edit";
                    return View(entity);
                }
                else
                {
                    ViewBag.IsEdit = false;
                    ViewBag.ActionTitle = "Create";
                    return View(Activator.CreateInstance<T>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {typeof(T).Name} for create/edit, id: {id}");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> CreateOrEdit(int? id, T entity)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (id.HasValue && id.Value > 0)
                    {
                        await _service.UpdateAsync(entity);
                        TempData["Success"] = "Item updated successfully.";
                    }
                    else
                    {
                        await _service.CreateAsync(entity);
                        TempData["Success"] = "Item created successfully.";
                    }
                    return RedirectToAction(nameof(Index));
                }
                
                ViewBag.IsEdit = id.HasValue && id.Value > 0;
                ViewBag.ActionTitle = ViewBag.IsEdit ? "Edit" : "Create";
                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving {typeof(T).Name}, id: {id}");
                ModelState.AddModelError("", "An error occurred while saving the item.");
                ViewBag.IsEdit = id.HasValue && id.Value > 0;
                ViewBag.ActionTitle = ViewBag.IsEdit ? "Edit" : "Create";
                return View(entity);
            }
        }

        // Keeping old methods for backward compatibility, but marking them as obsolete
        [Obsolete("Use CreateOrEdit instead")]
        [HttpGet]
        public virtual async Task<IActionResult> Create()
        {
            return await CreateOrEdit(null);
        }

        [Obsolete("Use CreateOrEdit instead")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(T entity)
        {
            return await CreateOrEdit(null, entity);
        }

        [Obsolete("Use CreateOrEdit instead")]
        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            return await CreateOrEdit(id);
        }

        [Obsolete("Use CreateOrEdit instead")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(int id, T entity)
        {
            return await CreateOrEdit(id, entity);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }
                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {typeof(T).Name} for delete, id: {id}");
                TempData["Error"] = "An error occurred while loading the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Item deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Item not found.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name}, id: {id}");
                TempData["Error"] = "An error occurred while deleting the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetJson(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }
                return Json(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {typeof(T).Name} as JSON, id: {id}");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllJson()
        {
            try
            {
                var entities = await _service.GetAllAsync();
                return Json(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all {typeof(T).Name} as JSON");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}