using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable(Constants.TableNames.AuditLogs);
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Action).IsRequired().HasMaxLength(50);
            builder.Property(e => e.OldValues).HasColumnType("TEXT");
            builder.Property(e => e.NewValues).HasColumnType("TEXT");
            builder.Property(e => e.ChangedProperties).HasColumnType("TEXT");
            builder.Property(e => e.UserName).HasMaxLength(100);
            builder.Property(e => e.IpAddress).HasMaxLength(50);
            builder.Property(e => e.UserAgent).HasMaxLength(255);
            builder.Property(e => e.Comments).HasMaxLength(500);
            
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.HasIndex(e => e.EntityName);
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => e.Timestamp);
        }
    }
}