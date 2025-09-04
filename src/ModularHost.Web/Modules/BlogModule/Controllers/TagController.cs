using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Controllers;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Modules.Blog.Controllers
{
    [Area("Blog")]
    [Route("[controller]")]
    public class TagController : BaseController<Tag>
    {
        public TagController(
            IRepository<Tag> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, unitOfWork, logger, userManager, httpContextAccessor)
        {
        }

        protected override string EntityName => "Tag";

        // Override CreateNewEntity to properly initialize required properties
        protected override Tag CreateNewEntity()
        {
            return new Tag
            {
                Name = string.Empty,
                Slug = string.Empty,
                Description = string.Empty,
                IsActive = true
            };
        }

        // Override Index to show tags with post count
        public override async Task<IActionResult> Index(int page = 1, string search = null)
        {
            IQueryable<Tag> query = _repository.Query()
                .Include(t => t.BlogPosts);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => 
                    t.Name.Contains(search) || 
                    t.Description.Contains(search) ||
                    t.Slug.Contains(search));
            }

            var filteredQuery = ApplyFilters(query);

            var totalItems = await filteredQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var tags = await filteredQuery
                .OrderBy(t => t.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new TagWithCount
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    Description = t.Description,
                    IsActive = t.IsActive,
                    PostCount = t.BlogPosts.Count(p => !p.IsDeleted && p.IsPublished),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.EntityName = EntityName;
            ViewBag.ControllerName = ControllerName;

            return View(tags);
        }

        // Override search implementation
        protected override IQueryable<Tag> ApplySearch(IQueryable<Tag> query, string search)
        {
            return query.Where(t =>
                t.Name.Contains(search) ||
                t.Slug.Contains(search) ||
                t.Description.Contains(search));
        }

        // Include related data
        protected override async Task<Tag> GetEntityWithIncludes(Guid id)
        {
            return await _repository.Query()
                .Include(t => t.BlogPosts)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Validate tag before save
        protected override async Task<bool> ValidateEntity(Tag entity, bool isEdit = false)
        {
            if (!await base.ValidateEntity(entity, isEdit))
                return false;

            // Check for duplicate slug
            var duplicateSlug = await _repository.Query()
                .AnyAsync(t => t.Slug == entity.Slug && t.Id != entity.Id && !t.IsDeleted);

            if (duplicateSlug)
            {
                ModelState.AddModelError("Slug", "A tag with this slug already exists.");
                return false;
            }

            // Check for duplicate name
            var duplicateName = await _repository.Query()
                .AnyAsync(t => t.Name == entity.Name && t.Id != entity.Id && !t.IsDeleted);

            if (duplicateName)
            {
                ModelState.AddModelError("Name", "A tag with this name already exists.");
                return false;
            }

            return true;
        }

        // Set slug before creating
        protected override async Task BeforeCreate(Tag entity)
        {
            // Generate slug if not provided
            if (string.IsNullOrWhiteSpace(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            await Task.CompletedTask;
        }

        // Update slug if name changed
        protected override async Task BeforeUpdate(Tag entity, Tag existingEntity)
        {
            // Regenerate slug if name changed and slug wasn't manually set
            if (entity.Name != existingEntity.Name && entity.Slug == existingEntity.Slug)
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            await Task.CompletedTask;
        }

        // Prevent deletion if tag has posts
        protected override async Task<bool> CanDelete(Tag entity)
        {
            // Check if tag has blog posts
            var hasPosts = entity.BlogPosts?.Any() ?? false;
            if (hasPosts)
            {
                TempData["Error"] = $"Cannot delete tag with {entity.BlogPosts.Count} blog post(s). Remove tag from posts first.";
                return false;
            }

            return true;
        }

        // AJAX: Get all tags for DataTable
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _repository.Query()
                .Include(t => t.BlogPosts)
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Slug,
                    t.Description,
                    t.IsActive,
                    PostCount = t.BlogPosts.Count(p => !p.IsDeleted && p.IsPublished),
                    t.CreatedAt
                })
                .ToListAsync();

            return Json(new { data = tags });
        }

        // AJAX: Get single tag for edit
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var tag = await _repository.GetByIdAsync(id);
            if (tag == null)
            {
                return Json(new { success = false, message = "Tag not found" });
            }

            return Json(new { success = true, data = tag });
        }

        // AJAX: Create tag
        [HttpPost("CreateAjax")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateAjax([FromBody] Tag model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                model.Id = Guid.NewGuid();
                model.CreatedAt = DateTime.UtcNow;
                model.CreatedBy = CurrentUserId;
                
                await BeforeCreate(model);
                
                if (!await ValidateEntity(model))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                await _repository.AddAsync(model);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Tag created: {Name} by {User}", model.Name, CurrentUserName);

                return Json(new { success = true, message = "Tag created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating tag", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Update tag
        [HttpPost("UpdateAjax")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateAjax([FromBody] Tag model)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(model.Id);
                if (existingEntity == null)
                {
                    return Json(new { success = false, message = "Tag not found" });
                }

                existingEntity.Name = model.Name;
                existingEntity.Slug = model.Slug;
                existingEntity.Description = model.Description;
                existingEntity.IsActive = model.IsActive;
                existingEntity.UpdatedAt = DateTime.UtcNow;
                existingEntity.UpdatedBy = CurrentUserId;

                await BeforeUpdate(existingEntity, existingEntity);
                
                if (!await ValidateEntity(existingEntity, true))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                _repository.Update(existingEntity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Tag updated: {Name} by {User}", model.Name, CurrentUserName);

                return Json(new { success = true, message = "Tag updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating tag", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Delete tag
        [HttpPost("DeleteAjax/{id}")]
        public async Task<IActionResult> DeleteAjax(Guid id)
        {
            try
            {
                var entity = await GetEntityWithIncludes(id);
                if (entity == null)
                {
                    return Json(new { success = false, message = "Tag not found" });
                }

                if (!await CanDelete(entity))
                {
                    return Json(new { success = false, message = TempData["Error"]?.ToString() ?? "Cannot delete this tag" });
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = CurrentUserId;
                _repository.Update(entity);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Tag deleted: {Name} by {User}", entity.Name, CurrentUserName);

                return Json(new { success = true, message = "Tag deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting tag", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Custom action for merging tags
        [HttpPost("merge")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> MergeTags(Guid sourceTagId, Guid targetTagId)
        {
            if (!IsAdmin)
            {
                return Forbid();
            }

            if (sourceTagId == targetTagId)
            {
                return Json(new { success = false, message = "Cannot merge a tag with itself." });
            }

            try
            {
                var sourceTag = await GetEntityWithIncludes(sourceTagId);
                var targetTag = await GetEntityWithIncludes(targetTagId);

                if (sourceTag == null || targetTag == null)
                {
                    return Json(new { success = false, message = "One or both tags not found." });
                }

                // Move all posts from source tag to target tag
                foreach (var post in sourceTag.BlogPosts.ToList())
                {
                    if (!targetTag.BlogPosts.Contains(post))
                    {
                        targetTag.BlogPosts.Add(post);
                    }
                    sourceTag.BlogPosts.Remove(post);
                    post.UpdatedAt = DateTime.UtcNow;
                    post.UpdatedBy = CurrentUserId;
                }

                // Mark source tag as deleted
                sourceTag.IsDeleted = true;
                sourceTag.UpdatedAt = DateTime.UtcNow;
                sourceTag.UpdatedBy = CurrentUserId;
                _repository.Update(sourceTag);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Tags merged: {SourceTag} into {TargetTag} by {User}", 
                    sourceTag.Name, targetTag.Name, CurrentUserName);

                TempData["Success"] = $"Tag '{sourceTag.Name}' merged into '{targetTag.Name}' successfully.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error merging tags", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Custom action for getting popular tags
        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularTags(int count = 10)
        {
            var popularTags = await _repository.Query()
                .Include(t => t.BlogPosts)
                .Where(t => !t.IsDeleted && t.IsActive)
                .OrderByDescending(t => t.BlogPosts.Count(p => !p.IsDeleted && p.IsPublished))
                .Take(count)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Slug,
                    PostCount = t.BlogPosts.Count(p => !p.IsDeleted && p.IsPublished)
                })
                .ToListAsync();

            return Json(popularTags);
        }

        // Custom action for auto-complete
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchTags(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new string[0]);
            }

            var tags = await _repository.Query()
                .Where(t => !t.IsDeleted && t.IsActive && t.Name.Contains(term))
                .OrderBy(t => t.Name)
                .Take(10)
                .Select(t => new
                {
                    id = t.Id,
                    text = t.Name,
                    slug = t.Slug
                })
                .ToListAsync();

            return Json(tags);
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

        // View model for tag with post count
        public class TagWithCount : Tag
        {
            public int PostCount { get; set; }
        }
    }
}