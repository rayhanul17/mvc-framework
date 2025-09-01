using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable(Constants.TableNames.RolePermissions);
            
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => new { e.RoleId, e.Url, e.HttpMethod }).IsUnique();
            builder.Property(e => e.Url).IsRequired().HasMaxLength(255);
            builder.Property(e => e.HttpMethod).HasMaxLength(10);
            
            builder.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}