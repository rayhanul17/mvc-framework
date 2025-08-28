using System;
using System.Collections.Generic;
using ModularHost.Web.Core.Models.Entities;

namespace ModularHost.Web.Modules.Blog.Models.Entities
{
    public class Tag : BaseEntity
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        
        public virtual ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
    }
}