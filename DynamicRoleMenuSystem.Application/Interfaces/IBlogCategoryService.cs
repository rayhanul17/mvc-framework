using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Application.Interfaces;

public interface IBlogCategoryService : IBaseService<BlogCategory>
{
    Task<Result<IEnumerable<BlogCategory>>> GetAllActiveAsync();
    Task<Result<BlogCategory>> GetBySlugAsync(string slug);
    Task<Result<string>> GenerateSlugAsync(string name);
    Task<Result<bool>> SlugExistsAsync(string slug, int? excludeId = null);
}