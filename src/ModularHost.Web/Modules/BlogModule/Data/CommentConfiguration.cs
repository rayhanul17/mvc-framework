using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Constants;

namespace MRCMS.Modules.Blog.Data
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable(TableNames.Comments);
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("TEXT");
            
            builder.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(e => e.PostId);
            builder.HasIndex(e => e.UserId);
        }
    }
}