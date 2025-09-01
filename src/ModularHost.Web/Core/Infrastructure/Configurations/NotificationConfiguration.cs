using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable(Constants.TableNames.Notifications);
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Message).HasColumnType("TEXT");
            builder.Property(e => e.TargetRoles).HasMaxLength(500);
            builder.Property(e => e.TargetUsers).HasMaxLength(500);
            builder.Property(e => e.ReadBy).HasColumnType("TEXT");
        }
    }
}