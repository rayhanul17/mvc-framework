using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Constants;

namespace ModularHost.Web.Modules.Blog.Data
{
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> builder)
        {
            builder.ToTable(TableNames.BlogPosts);
            
            builder.HasKey(e => e.Id);
            
            builder.HasIndex(e => e.Slug).IsUnique();
            builder.HasIndex(e => e.PublishedAt);
            
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.Summary)
                .HasMaxLength(500);
            
            builder.Property(e => e.Content)
                .HasColumnType("LONGTEXT");
            
            builder.Property(e => e.FeaturedImage)
                .HasMaxLength(500);
            
            builder.Property(e => e.MetaDescription)
                .HasMaxLength(255);
            
            builder.Property(e => e.MetaKeywords)
                .HasMaxLength(500);
            
            builder.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasMany(e => e.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}