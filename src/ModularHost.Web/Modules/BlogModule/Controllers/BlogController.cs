using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MRCMS.Core.Controllers;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Core.Extensions;
using MRCMS.Core.Helpers;
using MRCMS.Services.Interfaces;
using MRCMS.Modules.Blog.Services;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Models.ViewModels;
using MRCMS.Modules.Blog.Models.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace MRCMS.Modules.Blog.Controllers
{
    [Area("Blog")]
    [Route("Blog")]
    public class BlogController : BaseControllerWithViewModels<BlogPost, BlogPostDto, CreateBlogPostDto, UpdateBlogPostDto>
    {
        private readonly BlogService _blogService;
        private readonly IPermissionHelper _permissionHelper;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Tag> _tagRepository;
        
        protected override string EntityName => "Blog Post";
        protected override string ViewPrefix => "";
        protected override int PageSize => 10;

        public BlogController(
            IRepository<BlogPost> repository,
            IUnitOfWork unitOfWork,
            ILoggerService logger,
            IMapper mapper,
            BlogService blogService, 
            IPermissionHelper permissionHelper,
            IRepository<Category> categoryRepository,
            IRepository<Tag> tagRepository,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, unitOfWork, logger, mapper, userManager, httpContextAccessor)
        {
            _blogService = blogService;
            _permissionHelper = permissionHelper;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
        }

        [HttpGet("")]
        public override async Task<IActionResult> Index(int page = 1, string search = null, string sortBy = null, bool sortDesc = false)
        {
            // Use base implementation with custom filter for published posts only
            return await base.Index(page, search, sortBy, sortDesc);
        }

        // Override base Details to work with both ID and slug
        [HttpGet("{id:guid}")]
        public override async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation("Viewing {EntityName} by ID - Id: {Id}", EntityName, id);
                
                var post = await _blogService.GetPostByIdAsync(id);
                if (post == null)
                {
                    _logger.LogWarning("{EntityName} not found - Id: {Id}", EntityName, id);
                    return NotFound();
                }

                await _blogService.IncrementViewCountAsync(post.Id);
                
                var viewModel = _mapper.Map<BlogPostDto>(post);
                return View("Details", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error viewing {EntityName} by ID - Id: {Id}", ex, EntityName, id);
                return View("Error");
            }
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            try
            {
                // Check if slug is actually a GUID
                if (Guid.TryParse(slug, out var id))
                {
                    return await Details(id);
                }

                _logger.LogInformation("Viewing {EntityName} by slug - Slug: {Slug}", EntityName, slug);
                
                var post = await _blogService.GetPostBySlugAsync(slug);
                if (post == null)
                {
                    _logger.LogWarning("{EntityName} not found - Slug: {Slug}", EntityName, slug);
                    return NotFound();
                }

                await _blogService.IncrementViewCountAsync(post.Id);
                
                var viewModel = _mapper.Map<BlogPostDto>(post);
                return View("Details", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error viewing {EntityName} by slug - Slug: {Slug}", ex, EntityName, slug);
                return View("Error");
            }
        }

        [HttpGet("create")]
        public override async Task<IActionResult> Create()
        {
            return await base.Create();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Create(CreateBlogPostDto model)
        {
            return await base.Create(model);
        }

        [HttpGet("edit/{id:guid}")]
        public override async Task<IActionResult> Edit(Guid id)
        {
            return await base.Edit(id);
        }

        [HttpPost("edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Edit(Guid id, UpdateBlogPostDto model)
        {
            return await base.Edit(id, model);
        }

        [HttpGet("delete/{id:guid}")]
        public override async Task<IActionResult> Delete(Guid id)
        {
            return await base.Delete(id);
        }
        
        [HttpPost("delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            return await base.DeleteConfirmed(id);
        }

        [HttpPost("{postId:guid}/comment")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(Guid postId, string body, IFormFile? attachment = null, Guid? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(body))
                return BadRequest("Comment body is required");

            var post = await _blogService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            var comment = new Comment
            {
                PostId = postId,
                ParentCommentId = parentCommentId,
                UserId = CurrentUserId ?? Guid.Empty,
                Body = body,
                IsApproved = true
            };

            // Handle file attachment if provided
            if (attachment != null && attachment.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comments");
                var uploadResult = await FileUploadHelper.UploadFileAsync(attachment, uploadPath, "document", true);

                if (uploadResult.Success)
                {
                    comment.AttachmentPath = uploadResult.FilePath;
                    comment.AttachmentFileName = uploadResult.OriginalFileName;
                    comment.AttachmentSize = uploadResult.FileSize;
                    comment.AttachmentContentType = uploadResult.ContentType;
                }
                else
                {
                    TempData["Error"] = uploadResult.ErrorMessage;
                    return RedirectToAction("DetailsBySlug", new { slug = post.Slug });
                }
            }

            await _blogService.AddCommentAsync(comment);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Comment", comment);
            }

            return RedirectToAction("DetailsBySlug", new { slug = post.Slug });
        }



        private string GenerateSlug(string title)
        {
            var slug = title.ToLower();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = slug.Trim('-');
            return slug;
        }

        
        #region Base Controller Overrides
        
        protected override IQueryable<BlogPost> ApplySearch(IQueryable<BlogPost> query, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;
                
            return query.Where(p => 
                p.Title.Contains(search) ||
                p.Summary.Contains(search) ||
                p.Content.Contains(search) ||
                p.Author.FirstName.Contains(search) ||
                p.Author.LastName.Contains(search));
        }
        
        protected override IQueryable<BlogPost> ApplyCustomFilters(IQueryable<BlogPost> query)
        {
            // Apply base soft delete filter
            query = base.ApplyCustomFilters(query);
            
            // For public views, only show published posts
            if (!IsAdmin)
            {
                query = query.Where(p => p.IsPublished);
            }
            
            return query;
        }
        
        protected override async Task<BlogPost> GetEntityWithIncludes(Guid id)
        {
            return await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        protected override async Task PopulateViewBagForCreate()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            ViewBag.Tags = await _tagRepository.GetAllAsync();
        }
        
        protected override async Task PopulateViewBagForEdit(BlogPost entity)
        {
            await PopulateViewBagForCreate();
        }
        
        protected override async Task ConfigureEntityForCreate(BlogPost entity, CreateBlogPostDto model)
        {
            entity.Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Title);
            entity.AuthorId = CurrentUserId ?? Guid.Empty;
            
            if (model.IsPublished && !entity.PublishedAt.HasValue)
            {
                entity.PublishedAt = DateTime.UtcNow;
            }
        }
        
        protected override async Task ConfigureEntityForEdit(BlogPost entity, UpdateBlogPostDto model)
        {
            entity.Slug = !string.IsNullOrEmpty(model.Slug) ? model.Slug : GenerateSlug(model.Title);
            
            if (model.IsPublished && !entity.PublishedAt.HasValue)
            {
                entity.PublishedAt = DateTime.UtcNow;
            }
            else if (!model.IsPublished)
            {
                entity.PublishedAt = null;
            }
            
            await Task.CompletedTask;
        }
        
        protected override async Task<bool> CanEditEntity(BlogPost entity)
        {
            return entity.AuthorId == CurrentUserId || IsAdmin;
        }
        
        protected override async Task<bool> CanDeleteEntity(BlogPost entity)
        {
            return entity.AuthorId == CurrentUserId || IsAdmin;
        }
        
        #endregion
    }
}