using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModularHost.Web.Core.Extensions;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Core.Services;

namespace ModularHost.Web.Modules.Blog.Services
{
    public class BlogPostService : BaseServiceWithDto<BlogPost, BlogPostDto, CreateBlogPostDto, UpdateBlogPostDto>, IBlogPostService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogPostService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task BeforeCreateAsync(BlogPost entity, CreateBlogPostDto dto)
        {
            // Set author from current user
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (Guid.TryParse(userId, out var authorId))
            {
                entity.AuthorId = authorId;
                entity.CreatedBy = authorId;
            }

            // Auto-generate slug if not provided
            if (string.IsNullOrEmpty(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Title);
            }

            // Set published date if publishing
            if (entity.IsPublished && !entity.PublishedAt.HasValue)
            {
                entity.PublishedAt = DateTime.UtcNow;
            }

            // Handle tags
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                entity.BlogPostTags = dto.TagIds.Select(tagId => new BlogPostTag
                {
                    TagId = tagId,
                    BlogPostId = entity.Id
                }).ToList();
            }
        }

        protected override async Task BeforeUpdateAsync(BlogPost entity, UpdateBlogPostDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (Guid.TryParse(userId, out var editorId))
            {
                entity.UpdatedBy = editorId;
            }

            // Handle publish state change
            if (dto.IsPublished && !entity.IsPublished)
            {
                entity.PublishedAt = DateTime.UtcNow;
            }
            else if (!dto.IsPublished && entity.IsPublished)
            {
                entity.PublishedAt = null;
            }

            // Update tags
            if (dto.TagIds != null)
            {
                // Remove existing tags
                var existingTags = await _unitOfWork.Repository<BlogPostTag>()
                    .FindAsync(pt => pt.BlogPostId == entity.Id);
                
                foreach (var tag in existingTags)
                {
                    _unitOfWork.Repository<BlogPostTag>().Remove(tag);
                }

                // Add new tags
                entity.BlogPostTags = dto.TagIds.Select(tagId => new BlogPostTag
                {
                    TagId = tagId,
                    BlogPostId = entity.Id
                }).ToList();
            }
        }

        public async Task<BlogPostDto?> GetBySlugAsync(string slug)
        {
            var post = await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            return _mapper.Map<BlogPostDto>(post);
        }

        public async Task<IEnumerable<BlogPostDto>> GetPublishedPostsAsync(int page, int pageSize, string? tag = null)
        {
            var query = _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.IsPublished && p.PublishedAt <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(p => p.BlogPostTags.Any(pt => pt.Tag.Slug == tag));
            }

            var posts = await query
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        }

        public async Task<IEnumerable<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, int page, int pageSize)
        {
            var posts = await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        }

        public async Task<IEnumerable<BlogPostDto>> GetPostsByTagAsync(Guid tagId, int page, int pageSize)
        {
            var posts = await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.BlogPostTags.Any(pt => pt.TagId == tagId) && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        }

        public async Task<IEnumerable<BlogPostDto>> GetRelatedPostsAsync(Guid postId, int count = 5)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                return Enumerable.Empty<BlogPostDto>();

            var relatedPosts = await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.Id != postId && p.IsPublished &&
                           (p.CategoryId == post.CategoryId ||
                            p.BlogPostTags.Any(pt => post.BlogPostTags.Select(t => t.TagId).Contains(pt.TagId))))
                .OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BlogPostDto>>(relatedPosts);
        }

        public async Task<bool> IncrementViewCountAsync(Guid postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                return false;

            post.ViewCount++;
            _repository.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishPostAsync(Guid postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null || post.IsPublished)
                return false;

            post.IsPublished = true;
            post.PublishedAt = DateTime.UtcNow;
            _repository.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishPostAsync(Guid postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null || !post.IsPublished)
                return false;

            post.IsPublished = false;
            post.PublishedAt = null;
            _repository.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            return title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("&", "and");
        }
    }
}