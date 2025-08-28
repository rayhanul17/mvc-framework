using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Core.Extensions;
using ModularHost.Web.Modules.Blog.Services;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Models.ViewModels;

namespace ModularHost.Web.Modules.Blog.Controllers
{
    [Area("Blog")]
    [Route("Blog")]
    public class BlogController : Controller
    {
        private readonly BlogService _blogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionHelper _permissionHelper;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Tag> _tagRepository;

        public BlogController(
            BlogService blogService, 
            IUnitOfWork unitOfWork, 
            IPermissionHelper permissionHelper,
            IRepository<Category> categoryRepository,
            IRepository<Tag> tagRepository)
        {
            _blogService = blogService;
            _unitOfWork = unitOfWork;
            _permissionHelper = permissionHelper;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string tag = null)
        {
            var posts = await _blogService.GetPublishedPostsAsync(page, 10, tag);
            return View(posts);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var post = await _blogService.GetPostBySlugAsync(slug);
            if (post == null)
                return NotFound();

            await _blogService.IncrementViewCountAsync(post.Id);
            return View(post);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new BlogPost { Title = "", Slug = "", Content = "" });
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = GenerateSlug(model.Title);
                model.AuthorId = GetCurrentUserId();
                model.IsPublished = true;
                model.PublishedAt = DateTime.UtcNow;
                
                await _blogService.CreatePostAsync(model);
                return RedirectToAction(nameof(Details), new { slug = model.Slug });
            }

            return View(model);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound();

            if (post.AuthorId != GetCurrentUserId() && !IsAdmin())
                return Forbid();

            return View(post);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BlogPost model)
        {
            if (id != model.Id)
                return NotFound();

            var existingPost = await _blogService.GetPostByIdAsync(id);
            if (existingPost == null)
                return NotFound();

            if (existingPost.AuthorId != GetCurrentUserId() && !IsAdmin())
                return Forbid();

            if (ModelState.IsValid)
            {
                existingPost.Title = model.Title;
                existingPost.Summary = model.Summary;
                existingPost.Content = model.Content;
                existingPost.FeaturedImage = model.FeaturedImage;
                
                await _blogService.UpdatePostAsync(existingPost);
                return RedirectToAction(nameof(Details), new { slug = existingPost.Slug });
            }

            return View(model);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound();

            if (post.AuthorId != GetCurrentUserId() && !IsAdmin())
                return Forbid();

            await _blogService.DeletePostAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{postId}/comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid postId, string body, Guid? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(body))
                return BadRequest();

            var post = await _blogService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            var comment = new Comment
            {
                PostId = postId,
                ParentCommentId = parentCommentId,
                UserId = GetCurrentUserId(),
                Body = body,
                IsApproved = true
            };

            await _blogService.AddCommentAsync(comment);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Comment", comment);
            }

            return RedirectToAction(nameof(Details), new { slug = post.Slug });
        }

        #region Category Management

        [HttpGet("categories")]
        [Authorize(Roles = "Admin,BlogAuthor")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return Json(new
            {
                id = category.Id,
                name = category.Name,
                slug = category.Slug,
                description = category.Description
            });
        }

        [HttpPost("category/save")]
        [Authorize(Roles = "Admin,BlogAuthor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                Category category;
                if (model.Id == Guid.Empty)
                {
                    category = new Category
                    {
                        Name = model.Name,
                        Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Name),
                        Description = model.Description
                    };
                    await _categoryRepository.AddAsync(category);
                }
                else
                {
                    category = await _categoryRepository.GetByIdAsync(model.Id);
                    if (category == null)
                        return Json(new { success = false, message = "Category not found" });

                    category.Name = model.Name;
                    category.Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Name);
                    category.Description = model.Description;
                    _categoryRepository.Update(category);
                }

                await _unitOfWork.CommitAsync();
                return Json(new { success = true, category });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("category/delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                _categoryRepository.Remove(category);
                await _unitOfWork.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Tag Management

        [HttpGet("tags")]
        [Authorize(Roles = "Admin,BlogAuthor")]
        public async Task<IActionResult> Tags()
        {
            var tags = await _tagRepository.GetAllAsync();
            return View(tags);
        }

        [HttpGet("tag/{id}")]
        public async Task<IActionResult> GetTag(Guid id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                return NotFound();

            return Json(new
            {
                id = tag.Id,
                name = tag.Name,
                slug = tag.Slug,
                description = tag.Description
            });
        }

        [HttpPost("tag/save")]
        [Authorize(Roles = "Admin,BlogAuthor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTag(TagViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                Tag tag;
                if (model.Id == Guid.Empty)
                {
                    tag = new Tag
                    {
                        Name = model.Name,
                        Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Name),
                        Description = model.Description
                    };
                    await _tagRepository.AddAsync(tag);
                }
                else
                {
                    tag = await _tagRepository.GetByIdAsync(model.Id);
                    if (tag == null)
                        return Json(new { success = false, message = "Tag not found" });

                    tag.Name = model.Name;
                    tag.Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Name);
                    tag.Description = model.Description;
                    _tagRepository.Update(tag);
                }

                await _unitOfWork.CommitAsync();
                return Json(new { success = true, tag });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("tag/delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            try
            {
                var tag = await _tagRepository.GetByIdAsync(id);
                if (tag == null)
                    return Json(new { success = false, message = "Tag not found" });

                _tagRepository.Remove(tag);
                await _unitOfWork.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        private string GenerateSlug(string title)
        {
            var slug = title.ToLower();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = slug.Trim('-');
            return slug;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Administrator") || User.IsInRole("BlogAuthor");
        }
    }
}