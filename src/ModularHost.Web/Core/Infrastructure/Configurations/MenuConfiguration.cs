using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable(Constants.TableNames.Menus);
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Url).HasMaxLength(255);
            builder.Property(e => e.Icon).HasMaxLength(50);
            builder.Property(e => e.ClaimType).HasMaxLength(100);
            
            builder.HasOne(e => e.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}