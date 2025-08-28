using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Constants;

namespace ModularHost.Web.Modules.Blog.Data
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable(TableNames.Categories);
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(e => e.Slug).IsUnique();
            
            builder.Property(e => e.Description)
                .HasMaxLength(500);
            
            builder.HasMany(e => e.BlogPosts)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}