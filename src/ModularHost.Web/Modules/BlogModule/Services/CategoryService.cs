using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Core.Services;

namespace ModularHost.Web.Modules.Blog.Services
{
    public class CategoryService : BaseServiceWithDto<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>, ICategoryService
    {
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, mapper)
        {
        }

        protected override async Task BeforeCreateAsync(Category entity, CreateCategoryDto dto)
        {
            // Auto-generate slug if not provided
            if (string.IsNullOrEmpty(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            // Ensure unique slug
            var slugExists = await _repository.ExistsAsync(c => c.Slug == entity.Slug);
            if (slugExists)
            {
                entity.Slug = $"{entity.Slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }

        protected override async Task BeforeUpdateAsync(Category entity, UpdateCategoryDto dto)
        {
            // Check if slug changed and ensure uniqueness
            if (!string.IsNullOrEmpty(dto.Slug) && dto.Slug != entity.Slug)
            {
                var slugExists = await _repository.ExistsAsync(c => c.Slug == dto.Slug && c.Id != entity.Id);
                if (slugExists)
                {
                    throw new InvalidOperationException($"A category with slug '{dto.Slug}' already exists.");
                }
            }
        }

        public async Task<CategoryDto> GetBySlugAsync(string slug)
        {
            var category = await _repository.Query()
                .Include(c => c.BlogPosts)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _repository.Query()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithPostCountAsync()
        {
            var categories = await _repository.Query()
                .Include(c => c.BlogPosts)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}