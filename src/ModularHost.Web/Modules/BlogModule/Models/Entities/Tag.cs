using System;
using System.Collections.Generic;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.Blog.Models.Entities
{
    public class Tag : BaseEntity
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        
        public virtual ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
        
        // Navigation property for easier access to blog posts
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}