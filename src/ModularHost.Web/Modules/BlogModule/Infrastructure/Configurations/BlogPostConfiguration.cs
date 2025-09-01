using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Modules.BlogModule.Infrastructure.Configurations
{
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> builder)
        {
            builder.ToTable(Blog.Constants.TableNames.BlogPosts);
            
            builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Slug).IsRequired().HasMaxLength(300);
            builder.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
            builder.Property(e => e.Summary).HasMaxLength(500);
            builder.Property(e => e.FeaturedImage).HasMaxLength(500);
            builder.Property(e => e.MetaTitle).HasMaxLength(255);
            builder.Property(e => e.MetaDescription).HasMaxLength(500);
            builder.Property(e => e.MetaKeywords).HasMaxLength(500);
            
            builder.HasIndex(e => e.Slug).IsUnique();
            builder.HasIndex(e => e.PublishedAt);
            
            builder.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}