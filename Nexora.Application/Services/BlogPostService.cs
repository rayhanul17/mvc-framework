using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class BlogPostService : BaseService<BlogPost>, IBlogPostService
{
    public BlogPostService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<IEnumerable<BlogPost>>> GetPublishedPostsAsync(int? categoryId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<BlogPost>()
                .GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Where(p => p.IsPublished);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var posts = await query
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            return Result<IEnumerable<BlogPost>>.Success(posts);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<BlogPost>>.Failure($"Error retrieving published posts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BlogPost>>> GetPostsByCategoryAsync(int categoryId)
    {
        try
        {
            var posts = await _unitOfWork.Repository<BlogPost>()
                .GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Result<IEnumerable<BlogPost>>.Success(posts);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<BlogPost>>.Failure($"Error retrieving posts by category: {ex.Message}");
        }
    }

    public async Task<Result<BlogPost>> GetBySlugAsync(string slug)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>()
                .GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
                return Result<BlogPost>.Failure("Post not found");

            return Result<BlogPost>.Success(post);
        }
        catch (Exception ex)
        {
            return Result<BlogPost>.Failure($"Error retrieving post: {ex.Message}");
        }
    }

    public async Task<Result<BlogPost>> GetPostWithCategoryAsync(int id)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>()
                .GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return Result<BlogPost>.Failure("Post not found");

            return Result<BlogPost>.Success(post);
        }
        catch (Exception ex)
        {
            return Result<BlogPost>.Failure($"Error retrieving post: {ex.Message}");
        }
    }

    public async Task<Result<string>> GenerateSlugAsync(string title)
    {
        try
        {
            var slug = GenerateSlug(title);
            var baseSlug = slug;
            var counter = 1;

            while (await SlugExistsInternalAsync(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return Result<string>.Success(slug);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error generating slug: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SlugExistsAsync(string slug, int? excludeId = null)
    {
        try
        {
            var query = _unitOfWork.Repository<BlogPost>()
                .GetQueryable()
                .Where(p => p.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            var exists = await query.AnyAsync();
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking slug existence: {ex.Message}");
        }
    }

    public async Task<Result> PublishPostAsync(int id)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(id);
            if (post == null)
                return Result.Failure("Post not found");

            post.IsPublished = true;
            post.PublishedDate = DateTime.UtcNow;
            
            _unitOfWork.Repository<BlogPost>().Update(post);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error publishing post: {ex.Message}");
        }
    }

    public async Task<Result> UnpublishPostAsync(int id)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(id);
            if (post == null)
                return Result.Failure("Post not found");

            post.IsPublished = false;
            
            _unitOfWork.Repository<BlogPost>().Update(post);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error unpublishing post: {ex.Message}");
        }
    }

    public async Task<Result> IncrementViewCountAsync(int id)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(id);
            if (post == null)
                return Result.Failure("Post not found");

            post.ViewCount++;
            
            _unitOfWork.Repository<BlogPost>().Update(post);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error incrementing view count: {ex.Message}");
        }
    }

    private async Task<bool> SlugExistsInternalAsync(string slug)
    {
        return await _unitOfWork.Repository<BlogPost>()
            .GetQueryable()
            .AnyAsync(p => p.Slug == slug);
    }

    private string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}