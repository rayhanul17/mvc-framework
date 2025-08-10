using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface IBlogCategoryService : IBaseService<BlogCategory>
{
    Task<Result<IEnumerable<BlogCategory>>> GetAllActiveAsync();
    Task<Result<BlogCategory>> GetBySlugAsync(string slug);
    Task<Result<string>> GenerateSlugAsync(string name);
    Task<Result<bool>> SlugExistsAsync(string slug, int? excludeId = null);
}