using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Constants;

namespace ModularHost.Web.Modules.Blog.Data
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable(TableNames.Tags);
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(e => e.Slug).IsUnique();
        }
    }
}