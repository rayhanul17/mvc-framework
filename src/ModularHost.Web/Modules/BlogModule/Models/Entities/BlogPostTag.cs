using System;

namespace MRCMS.Modules.Blog.Models.Entities
{
    public class BlogPostTag
    {
        public Guid BlogPostId { get; set; }
        public Guid TagId { get; set; }
        
        public virtual BlogPost BlogPost { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}