using System;
using System.Collections.Generic;
using MRCMS.Modules.Blog.Models.Entities;

namespace MRCMS.Modules.Blog.Models.DTOs
{
    public class BlogPostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
        public int CommentCount { get; set; }
        
        // Navigation properties for views
        public CategoryDto? Category { get; set; }
        public AuthorDto? Author { get; set; }
        public List<BlogPostTagDto> BlogPostTags { get; set; } = new List<BlogPostTagDto>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public string? TagNames { get; set; }
    }

    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class BlogPostTagDto
    {
        public Guid BlogPostId { get; set; }
        public Guid TagId { get; set; }
        public TagDto Tag { get; set; } = null!;
    }

    public class CreateBlogPostDto
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Summary { get; set; }
        public required string Content { get; set; }
        public string? FeaturedImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new List<Guid>();
    }

    public class UpdateBlogPostDto
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string Summary { get; set; }
        public required string Content { get; set; }
        public string? FeaturedImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new List<Guid>();
    }
}