using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Modules.Blog.Models.Entities;

namespace MRCMS.Modules.BlogModule.Infrastructure.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable(Blog.Constants.TableNames.Tags);
            
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Slug).IsRequired().HasMaxLength(120);
            builder.Property(e => e.Description).HasMaxLength(500);
            
            builder.HasIndex(e => e.Slug).IsUnique();
        }
    }
}