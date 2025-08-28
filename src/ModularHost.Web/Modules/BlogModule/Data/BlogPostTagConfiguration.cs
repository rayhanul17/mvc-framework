using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Constants;

namespace ModularHost.Web.Modules.Blog.Data
{
    public class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
    {
        public void Configure(EntityTypeBuilder<BlogPostTag> builder)
        {
            builder.ToTable(TableNames.BlogPostTags);
            
            builder.HasKey(e => new { e.BlogPostId, e.TagId });
            
            builder.HasOne(e => e.BlogPost)
                .WithMany(p => p.BlogPostTags)
                .HasForeignKey(e => e.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.Tag)
                .WithMany(t => t.BlogPostTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}