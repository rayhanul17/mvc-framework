using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class LogArchiveConfiguration : IEntityTypeConfiguration<LogArchive>
    {
        public void Configure(EntityTypeBuilder<LogArchive> builder)
        {
            builder.ToTable(Constants.TableNames.LogArchives);
            
            builder.HasIndex(e => e.ArchivedAt);
        }
    }
}