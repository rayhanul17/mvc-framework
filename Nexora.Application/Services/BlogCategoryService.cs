using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class BlogCategoryService : BaseService<BlogCategory>, IBlogCategoryService
{
    public BlogCategoryService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<IEnumerable<BlogCategory>>> GetAllActiveAsync()
    {
        try
        {
            var categories = await _unitOfWork.Repository<BlogCategory>()
                .GetQueryable()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return Result<IEnumerable<BlogCategory>>.Success(categories);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<BlogCategory>>.Failure($"Error retrieving active categories: {ex.Message}");
        }
    }

    public async Task<Result<BlogCategory>> GetBySlugAsync(string slug)
    {
        try
        {
            var category = await _unitOfWork.Repository<BlogCategory>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return Result<BlogCategory>.Failure("Category not found");

            return Result<BlogCategory>.Success(category);
        }
        catch (Exception ex)
        {
            return Result<BlogCategory>.Failure($"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<Result<string>> GenerateSlugAsync(string name)
    {
        try
        {
            var slug = GenerateSlug(name);
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
            var query = _unitOfWork.Repository<BlogCategory>()
                .GetQueryable()
                .Where(c => c.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            var exists = await query.AnyAsync();
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking slug existence: {ex.Message}");
        }
    }

    private async Task<bool> SlugExistsInternalAsync(string slug)
    {
        return await _unitOfWork.Repository<BlogCategory>()
            .GetQueryable()
            .AnyAsync(c => c.Slug == slug);
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