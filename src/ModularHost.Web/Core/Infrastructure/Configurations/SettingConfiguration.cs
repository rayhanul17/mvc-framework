using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable(Constants.TableNames.Settings);
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Key).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Value).IsRequired().HasColumnType("TEXT");
            builder.Property(e => e.Description).HasMaxLength(500);
            builder.Property(e => e.Category).HasMaxLength(50);
            
            builder.HasIndex(e => e.Key).IsUnique();
            builder.HasIndex(e => e.Category);
        }
    }
}