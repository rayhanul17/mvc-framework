using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Services;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Services.Interfaces;
using MRCMS.Modules.Blog.Models.DTOs;
using AutoMapper;

namespace MRCMS.Modules.Blog.Services
{
    public class TagService : BaseServiceWithDto<Tag, TagDto, CreateTagDto, UpdateTagDto>, ITagService
    {
        public TagService(IRepository<Tag> repository, IUnitOfWork unitOfWork, IMapper mapper)
            : base(repository, unitOfWork, mapper)
        {
        }

        public async Task<TagDto> GetBySlugAsync(string slug)
        {
            var tag = await _repository.Query()
                .FirstOrDefaultAsync(t => t.Slug == slug);
            return _mapper.Map<TagDto>(tag);
        }

        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count)
        {
            var tags = await _repository.Query()
                .Include(t => t.BlogPostTags)
                .OrderByDescending(t => t.BlogPostTags.Count)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }

        public async Task<IEnumerable<TagDto>> GetTagsWithPostCountAsync()
        {
            var tags = await _repository.Query()
                .Include(t => t.BlogPostTags)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    PostCount = t.BlogPostTags.Count,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .OrderByDescending(t => t.PostCount)
                .ToListAsync();

            return tags;
        }

        protected override async Task BeforeCreateAsync(Tag entity, CreateTagDto dto)
        {
            // Generate slug if not provided
            if (string.IsNullOrEmpty(dto.Slug))
            {
                entity.Slug = GenerateSlug(dto.Name);
            }
            await base.BeforeCreateAsync(entity, dto);
        }

        protected override async Task BeforeUpdateAsync(Tag entity, UpdateTagDto dto)
        {
            // Update slug if name changed and slug not provided
            if (string.IsNullOrEmpty(dto.Slug) && entity.Name != dto.Name)
            {
                entity.Slug = GenerateSlug(dto.Name);
            }
            await base.BeforeUpdateAsync(entity, dto);
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Convert to lowercase
            var slug = text.ToLowerInvariant();

            // Remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

            // Remove multiple hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from ends
            slug = slug.Trim('-');

            return slug;
        }
    }
}