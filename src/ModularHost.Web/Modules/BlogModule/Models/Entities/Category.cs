using System;
using System.Collections.Generic;
using ModularHost.Web.Core.Models.Entities;

namespace ModularHost.Web.Modules.Blog.Models.Entities
{
    public class Category : BaseEntity
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}