using System;

namespace MRCMS.Modules.Blog.Models.DTOs
{
    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTagDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
    }

    public class UpdateTagDto
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
    }
}