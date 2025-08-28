using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Core.Services.Interfaces;

namespace ModularHost.Web.Modules.Blog.Services.Interfaces
{
    public interface ITagService : IBaseService<Tag, TagDto, CreateTagDto, UpdateTagDto>
    {
        Task<TagDto> GetBySlugAsync(string slug);
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<TagDto>> GetTagsWithPostCountAsync();
    }
}