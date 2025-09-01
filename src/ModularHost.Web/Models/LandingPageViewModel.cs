using MRCMS.Modules.Blog.Models.DTOs;
using MRCMS.Modules.Blog.Models.Entities;

namespace MRCMS.Models
{
    public class LandingPageViewModel
    {
        public List<BlogPostDto> Posts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
        public string? CurrentCategory { get; set; }
        public string? CurrentTag { get; set; }
        public string? SearchQuery { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPosts { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}