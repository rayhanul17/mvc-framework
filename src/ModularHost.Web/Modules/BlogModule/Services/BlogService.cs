using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Core.Services;
using MRCMS.Modules.Blog.Models.Entities;

namespace MRCMS.Modules.Blog.Services
{
    public class BlogService : BaseService<BlogPost>
    {
        private readonly IRepository<Comment> _commentRepository;

        public BlogService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _commentRepository = unitOfWork.Repository<Comment>();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync(int page, int pageSize, string? tag = null)
        {
            var query = _repository.Query()
                .Where(p => p.IsPublished && p.PublishedAt <= DateTime.UtcNow)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(p => p.BlogPostTags.Any(bpt => bpt.Tag.Name == tag));
            }

            return await query
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            return await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<BlogPost?> GetPostByIdAsync(Guid id)
        {
            return await _repository.Query()
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<BlogPost> CreatePostAsync(BlogPost post)
        {
            await _repository.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();
            return post;
        }

        public async Task<BlogPost> UpdatePostAsync(BlogPost post)
        {
            _repository.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            var post = await GetPostByIdAsync(id);
            if (post == null)
                return false;

            _repository.Remove(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task IncrementViewCountAsync(Guid postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post != null)
            {
                post.ViewCount++;
                _repository.Update(post);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _commentRepository.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return comment;
        }

        public async Task<IEnumerable<Comment>> GetCommentsForPostAsync(Guid postId)
        {
            return await _commentRepository.Query()
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            var allTags = await _repository.Query()
                .Where(p => p.IsPublished)
                .SelectMany(p => p.BlogPostTags.Select(bpt => bpt.Tag.Name))
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return allTags;
        }

        public async Task<int> GetPostCountAsync(string? tag = null)
        {
            var query = _repository.Query()
                .Where(p => p.IsPublished && p.PublishedAt <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(p => p.BlogPostTags.Any(bpt => bpt.Tag.Name == tag));
            }

            return await query.CountAsync();
        }
    }
}