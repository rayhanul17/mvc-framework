using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MRCMS.Core.Controllers;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class MenuController : BaseController<Menu>
    {
        public MenuController(
            IRepository<Menu> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, unitOfWork, logger, userManager, httpContextAccessor)
        {
        }

        protected override string EntityName => "Menu";

        // Override to include children in the index view
        public override async Task<IActionResult> Index(int page = 1, string search = null)
        {
            IQueryable<Menu> query = _repository.Query()
                .Include(m => m.Children)
                    .ThenInclude(c => c.Children)
                        .ThenInclude(gc => gc.Children); // Load up to 3 levels deep

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = ApplySearch(query, search);
            }

            query = ApplyFilters(query);

            var menus = await query
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Order)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;

            return View(menus);
        }

        // Override search implementation
        protected override IQueryable<Menu> ApplySearch(IQueryable<Menu> query, string search)
        {
            return query.Where(m =>
                m.Title.Contains(search) ||
                m.Url.Contains(search) ||
                m.Icon.Contains(search));
        }

        // Include related data
        protected override async Task<Menu> GetEntityWithIncludes(Guid id)
        {
            return await _repository.Query()
                .Include(m => m.Parent)
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Populate parent menus dropdown with hierarchy
        protected override async Task PopulateViewBag()
        {
            var allMenus = await _repository.Query()
                .Where(m => !m.IsDeleted)
                .Include(m => m.Children)
                .OrderBy(m => m.Order)
                .ToListAsync();

            var parentMenuOptions = new List<dynamic>();
            BuildHierarchicalMenuList(allMenus.Where(m => m.ParentId == null), allMenus, parentMenuOptions, 0);

            ViewBag.ParentMenus = parentMenuOptions;
        }

        private void BuildHierarchicalMenuList(IEnumerable<Menu> menus, List<Menu> allMenus, List<dynamic> result, int level)
        {
            foreach (var menu in menus)
            {
                var prefix = level > 0 ? new string('—', level) + " " : "";
                result.Add(new { menu.Id, Title = prefix + menu.Title });
                
                var children = allMenus.Where(m => m.ParentId == menu.Id);
                if (children.Any())
                {
                    BuildHierarchicalMenuList(children, allMenus, result, level + 1);
                }
            }
        }

        // Validate menu before save
        protected override async Task<bool> ValidateEntity(Menu entity, bool isEdit = false)
        {
            if (!await base.ValidateEntity(entity, isEdit))
                return false;

            // Check for circular reference
            if (entity.ParentId.HasValue && entity.ParentId == entity.Id)
            {
                ModelState.AddModelError("ParentId", "A menu cannot be its own parent.");
                return false;
            }

            // Validate parent exists
            if (entity.ParentId.HasValue)
            {
                var parentExists = await _repository.Query()
                    .AnyAsync(m => m.Id == entity.ParentId.Value && !m.IsDeleted);

                if (!parentExists)
                {
                    ModelState.AddModelError("ParentId", "Selected parent menu does not exist.");
                    return false;
                }
            }

            return true;
        }

        // Set order before creating
        protected override async Task BeforeCreate(Menu entity)
        {
            // Auto-set display order
            var maxOrder = await _repository.Query()
                .Where(m => m.ParentId == entity.ParentId && !m.IsDeleted)
                .MaxAsync(m => (int?)m.Order) ?? 0;

            entity.Order = maxOrder + 1;
            
            // Set ActiveUrl to Url if not provided
            if (string.IsNullOrWhiteSpace(entity.ActiveUrl))
            {
                entity.ActiveUrl = entity.Url;
            }
        }

        // Handle ActiveUrl before update
        protected override async Task BeforeUpdate(Menu entity, Menu existingEntity)
        {
            // Set ActiveUrl to Url if not provided
            if (string.IsNullOrWhiteSpace(entity.ActiveUrl))
            {
                entity.ActiveUrl = entity.Url;
            }
            
            await base.BeforeUpdate(entity, existingEntity);
        }

        // Prevent deletion if menu has children
        protected override async Task<bool> CanDelete(Menu entity)
        {
            var hasChildren = await _repository.Query()
                .AnyAsync(m => m.ParentId == entity.Id && !m.IsDeleted);

            if (hasChildren)
            {
                TempData["Error"] = "Cannot delete menu with child items. Delete child items first.";
                return false;
            }

            return true;
        }

        // Custom action for updating menu order
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest request)
        {
            try
            {
                foreach (var item in request.Items)
                {
                    var menu = await _repository.GetByIdAsync(item.Id);
                    if (menu != null)
                    {
                        menu.Order = item.Order;
                        menu.ParentId = item.ParentId;
                        menu.UpdatedAt = DateTime.UtcNow;
                        _repository.Update(menu);
                    }
                }

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Menu order updated successfully");
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating menu order", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateOrderRequest
        {
            public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
        }

        public class OrderItem
        {
            public Guid Id { get; set; }
            public int Order { get; set; }
            public Guid? ParentId { get; set; }
        }
    }
}