using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRCMS.Modules.Blog.Models.DTOs;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Core.Services.Interfaces;

namespace MRCMS.Modules.Blog.Services.Interfaces
{
    public interface ITagService : IBaseService<Tag, TagDto, CreateTagDto, UpdateTagDto>
    {
        Task<TagDto> GetBySlugAsync(string slug);
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<TagDto>> GetTagsWithPostCountAsync();
    }
}