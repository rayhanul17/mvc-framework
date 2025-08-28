using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Core.Services.Interfaces;

namespace ModularHost.Web.Modules.Blog.Services.Interfaces
{
    public interface IBlogPostService : IBaseService<BlogPost, BlogPostDto, CreateBlogPostDto, UpdateBlogPostDto>
    {
        Task<BlogPostDto?> GetBySlugAsync(string slug);
        Task<IEnumerable<BlogPostDto>> GetPublishedPostsAsync(int page, int pageSize, string? tag = null);
        Task<IEnumerable<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, int page, int pageSize);
        Task<IEnumerable<BlogPostDto>> GetPostsByTagAsync(Guid tagId, int page, int pageSize);
        Task<IEnumerable<BlogPostDto>> GetRelatedPostsAsync(Guid postId, int count = 5);
        Task<bool> IncrementViewCountAsync(Guid postId);
        Task<bool> PublishPostAsync(Guid postId);
        Task<bool> UnpublishPostAsync(Guid postId);
    }
}