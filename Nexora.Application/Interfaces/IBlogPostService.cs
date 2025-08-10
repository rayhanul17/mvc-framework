using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface IBlogPostService : IBaseService<BlogPost>
{
    Task<Result<IEnumerable<BlogPost>>> GetPublishedPostsAsync(int? categoryId = null);
    Task<Result<IEnumerable<BlogPost>>> GetPostsByCategoryAsync(int categoryId);
    Task<Result<BlogPost>> GetBySlugAsync(string slug);
    Task<Result<BlogPost>> GetPostWithCategoryAsync(int id);
    Task<Result<string>> GenerateSlugAsync(string title);
    Task<Result<bool>> SlugExistsAsync(string slug, int? excludeId = null);
    Task<Result> PublishPostAsync(int id);
    Task<Result> UnpublishPostAsync(int id);
    Task<Result> IncrementViewCountAsync(int id);
}