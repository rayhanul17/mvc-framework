using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable(Constants.TableNames.Logs);
            
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.CreatedAt);
            builder.Property(e => e.Level).HasMaxLength(50);
            builder.Property(e => e.Message).HasMaxLength(4000);
            builder.Property(e => e.Exception).HasColumnType("TEXT");
            builder.Property(e => e.Properties).HasColumnType("TEXT");
        }
    }
}