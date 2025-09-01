using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Controllers;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.Blog.Controllers
{
    [Area("Blog")]
    [Route("Blog/Category")]
    [Authorize(Roles = "Admin,BlogAuthor,SuperAdmin")]
    public class CategoryController : BaseController<Category>
    {
        public CategoryController(
            IRepository<Category> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, unitOfWork, logger, userManager, httpContextAccessor)
        {
        }

        protected override string EntityName => "Category";

        // Override Index to show category hierarchy
        public override async Task<IActionResult> Index(int page = 1, string search = null)
        {
            IQueryable<Category> query = _repository.Query()
                .Include(c => c.Parent)
                .Include(c => c.Children);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => 
                    c.Name.Contains(search) || 
                    c.Description.Contains(search) ||
                    c.Slug.Contains(search));
            }

            var filteredQuery = ApplyFilters(query);

            var totalItems = await filteredQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var categories = await filteredQuery
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;

            return View(categories);
        }

        // Override search implementation
        protected override IQueryable<Category> ApplySearch(IQueryable<Category> query, string search)
        {
            return query.Where(c =>
                c.Name.Contains(search) ||
                c.Slug.Contains(search) ||
                c.Description.Contains(search));
        }

        // Include related data
        protected override async Task<Category> GetEntityWithIncludes(Guid id)
        {
            return await _repository.Query()
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .Include(c => c.BlogPosts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Populate parent categories dropdown
        protected override async Task PopulateViewBag()
        {
            var parentCategories = await _repository.Query()
                .Where(c => !c.IsDeleted && c.ParentId == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.ParentCategories = parentCategories;
        }

        // Validate category before save
        protected override async Task<bool> ValidateEntity(Category entity, bool isEdit = false)
        {
            if (!await base.ValidateEntity(entity, isEdit))
                return false;

            // Check for duplicate slug
            var duplicateSlug = await _repository.Query()
                .AnyAsync(c => c.Slug == entity.Slug && c.Id != entity.Id && !c.IsDeleted);

            if (duplicateSlug)
            {
                ModelState.AddModelError("Slug", "A category with this slug already exists.");
                return false;
            }

            // Check for duplicate name
            var duplicateName = await _repository.Query()
                .AnyAsync(c => c.Name == entity.Name && c.Id != entity.Id && !c.IsDeleted);

            if (duplicateName)
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
                return false;
            }

            // Check for circular reference
            if (entity.ParentId.HasValue && entity.ParentId == entity.Id)
            {
                ModelState.AddModelError("ParentId", "A category cannot be its own parent.");
                return false;
            }

            // Validate parent exists
            if (entity.ParentId.HasValue)
            {
                var parentExists = await _repository.Query()
                    .AnyAsync(c => c.Id == entity.ParentId.Value && !c.IsDeleted);

                if (!parentExists)
                {
                    ModelState.AddModelError("ParentId", "Selected parent category does not exist.");
                    return false;
                }

                // Check for circular hierarchy
                if (isEdit && await IsCircularHierarchy(entity.Id, entity.ParentId.Value))
                {
                    ModelState.AddModelError("ParentId", "This would create a circular hierarchy.");
                    return false;
                }
            }

            return true;
        }

        // Check for circular hierarchy
        private async Task<bool> IsCircularHierarchy(Guid categoryId, Guid proposedParentId)
        {
            var parent = await _repository.GetByIdAsync(proposedParentId);
            while (parent != null)
            {
                if (parent.Id == categoryId)
                    return true;
                
                if (parent.ParentId.HasValue)
                    parent = await _repository.GetByIdAsync(parent.ParentId.Value);
                else
                    break;
            }
            return false;
        }

        // Set order and slug before creating
        protected override async Task BeforeCreate(Category entity)
        {
            // Generate slug if not provided
            if (string.IsNullOrWhiteSpace(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            // Auto-set display order
            var maxOrder = await _repository.Query()
                .Where(c => c.ParentId == entity.ParentId && !c.IsDeleted)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

            entity.DisplayOrder = maxOrder + 1;
        }

        // Update slug if name changed
        protected override async Task BeforeUpdate(Category entity, Category existingEntity)
        {
            // Regenerate slug if name changed and slug wasn't manually set
            if (entity.Name != existingEntity.Name && entity.Slug == existingEntity.Slug)
            {
                entity.Slug = GenerateSlug(entity.Name);
            }
        }

        // Prevent deletion if category has posts
        protected override async Task<bool> CanDelete(Category entity)
        {
            // Check if category has children
            var hasChildren = await _repository.Query()
                .AnyAsync(c => c.ParentId == entity.Id && !c.IsDeleted);

            if (hasChildren)
            {
                TempData["Error"] = "Cannot delete category with child categories. Delete child categories first.";
                return false;
            }

            // Check if category has blog posts
            var hasPosts = entity.BlogPosts?.Any() ?? false;
            if (hasPosts)
            {
                TempData["Error"] = $"Cannot delete category with {entity.BlogPosts.Count} blog post(s). Remove or reassign posts first.";
                return false;
            }

            return true;
        }

        // Custom action for updating category order
        [HttpPost("update-order")]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest request)
        {
            if (!IsAdmin)
            {
                return Forbid();
            }

            try
            {
                foreach (var item in request.Items)
                {
                    var category = await _repository.GetByIdAsync(item.Id);
                    if (category != null)
                    {
                        category.DisplayOrder = item.Order;
                        category.ParentId = item.ParentId;
                        category.UpdatedAt = DateTime.UtcNow;
                        category.UpdatedBy = CurrentUserId;
                        _repository.Update(category);
                    }
                }

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Category order updated by {User}", CurrentUserName);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating category order", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to generate slug
        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var slug = text.ToLower();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = slug.Trim('-');
            return slug;
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