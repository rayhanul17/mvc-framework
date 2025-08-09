using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<string>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<string>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Menu> Menus { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }
    
    // Customer Support Entities
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<TicketNotification> TicketNotifications { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        builder.Entity<Menu>(entity =>
        {
            entity.ToTable("Menus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Area).HasMaxLength(50);
            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.Order);
        });

        builder.Entity<RoleMenu>(entity =>
        {
            entity.ToTable("RoleMenus");
            entity.HasKey(e => e.Id);

            entity.HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId)
                .IsRequired();

            entity.HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId)
                .IsRequired();

            entity.HasIndex(e => new { e.RoleId, e.MenuId }).IsUnique();
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        builder.Entity<BlogCategory>(entity =>
        {
            entity.ToTable("BlogCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.DisplayOrder);
        });

        builder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("BlogPosts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.FeaturedImageUrl).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.MetaTitle).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.Property(e => e.MetaKeywords).HasMaxLength(500);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.IsPublished);
        });

        builder.Entity<SiteSetting>(entity =>
        {
            entity.ToTable("SiteSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasConversion<string>();
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.ValidValues).HasMaxLength(1000);
            entity.Property(e => e.IsRequired).IsRequired();
            entity.Property(e => e.IsSystemSetting).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
            
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Order);
        });

        // Ticket Configuration
        builder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.TicketNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedBy)
                .WithMany()
                .HasForeignKey(e => e.AssignedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
        });

        // TicketComment Configuration
        builder.Entity<TicketComment>(entity =>
        {
            entity.ToTable("TicketComments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.OldStatus).HasConversion<string>();
            entity.Property(e => e.NewStatus).HasConversion<string>();

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TicketId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // TicketAttachment Configuration
        builder.Entity<TicketAttachment>(entity =>
        {
            entity.ToTable("TicketAttachments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(50);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Comment)
                .WithMany(c => c.Attachments)
                .HasForeignKey(e => e.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TicketNotification Configuration
        builder.Entity<TicketNotification>(entity =>
        {
            entity.ToTable("TicketNotifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Notifications)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
        });

        // TicketHistory Configuration
        builder.Entity<TicketHistory>(entity =>
        {
            entity.ToTable("TicketHistory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.OldValue).HasMaxLength(100);
            entity.Property(e => e.NewValue).HasMaxLength(100);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.History)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TicketId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ApplicationUser || e.Entity is ApplicationRole || 
                       e.Entity is Menu || e.Entity is RoleMenu || 
                       e.Entity is BlogCategory || e.Entity is BlogPost || 
                       e.Entity is SiteSetting);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ApplicationUser user)
                        user.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is ApplicationRole role)
                        role.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Menu menu)
                        menu.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is RoleMenu roleMenu)
                        roleMenu.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is BlogCategory category)
                        category.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is BlogPost post)
                        post.CreatedAt = DateTime.UtcNow;
                    else if (entry.Entity is SiteSetting setting)
                        setting.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    if (entry.Entity is ApplicationUser modUser)
                        modUser.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is ApplicationRole modRole)
                        modRole.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Menu modMenu)
                        modMenu.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is RoleMenu modRoleMenu)
                        modRoleMenu.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is BlogCategory modCategory)
                        modCategory.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is BlogPost modPost)
                        modPost.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is SiteSetting modSetting)
                        modSetting.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}