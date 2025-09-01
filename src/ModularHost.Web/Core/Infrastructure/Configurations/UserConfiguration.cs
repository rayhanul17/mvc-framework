using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(Constants.TableNames.Users);
            
            builder.Property(e => e.FullName).HasMaxLength(200);
            builder.Property(e => e.Avatar).HasMaxLength(500);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.Address).HasMaxLength(255);
            builder.Property(e => e.City).HasMaxLength(100);
            builder.Property(e => e.Country).HasMaxLength(100);
            builder.Property(e => e.PostalCode).HasMaxLength(20);
            builder.Property(e => e.ProfilePicture).HasMaxLength(500);
        }
    }
}