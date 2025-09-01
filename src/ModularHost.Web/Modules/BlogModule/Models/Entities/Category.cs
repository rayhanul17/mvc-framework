using System;
using System.Collections.Generic;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.Blog.Models.Entities
{
    public class Category : BaseEntity
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        
        // Hierarchy
        public Guid? ParentId { get; set; }
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        
        // Relations
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}