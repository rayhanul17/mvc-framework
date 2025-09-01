using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRCMS.Modules.Blog.Models.DTOs;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Core.Services.Interfaces;

namespace MRCMS.Modules.Blog.Services.Interfaces
{
    public interface ICategoryService : IBaseService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
    {
        Task<CategoryDto> GetBySlugAsync(string slug);
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetCategoriesWithPostCountAsync();
    }
}