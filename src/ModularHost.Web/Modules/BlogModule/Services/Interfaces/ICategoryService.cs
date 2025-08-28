using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Core.Services.Interfaces;

namespace ModularHost.Web.Modules.Blog.Services.Interfaces
{
    public interface ICategoryService : IBaseService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
    {
        Task<CategoryDto> GetBySlugAsync(string slug);
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetCategoriesWithPostCountAsync();
    }
}