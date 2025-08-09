using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<string>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<string>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<string>>
{
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
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
    
    // Audit Log Entities
    public DbSet<Log> Logs { get; set; }
    public DbSet<LogArchive> LogArchives { get; set; }

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
        
        // Log Configuration
        builder.Entity<Log>(entity =>
        {
            entity.ToTable("Logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Changes).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.TableName);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.LoggedAt);
            entity.HasIndex(e => new { e.TableName, e.EntityId });
        });
        
        // LogArchive Configuration
        builder.Entity<LogArchive>(entity =>
        {
            entity.ToTable("LogArchives");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Changes).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.TableName);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.LoggedAt);
            entity.HasIndex(e => e.ArchivedAt);
            entity.HasIndex(e => new { e.TableName, e.EntityId });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture audit entries before saving
        var auditEntries = new List<AuditEntry>();
        
        // Get HTTP context to access user information
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString();

        foreach (var entry in ChangeTracker.Entries())
        {
            // Skip audit entities to prevent recursion
            if (entry.Entity is Log || entry.Entity is LogArchive)
                continue;
                
            // Only audit specific entities
            if (!(entry.Entity is ApplicationUser || entry.Entity is ApplicationRole || 
                 entry.Entity is Menu || entry.Entity is RoleMenu || 
                 entry.Entity is BlogCategory || entry.Entity is BlogPost || 
                 entry.Entity is SiteSetting || entry.Entity is Ticket || 
                 entry.Entity is TicketComment))
                continue;

            // Update timestamps
            UpdateTimestamps(entry);

            // Create audit entries
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                auditEntries.Add(CreateAuditEntry(entry, userId, ipAddress, userAgent));
            }
        }

        // Save the changes first
        var result = await base.SaveChangesAsync(cancellationToken);

        // Then save audit logs (after we have entity IDs for new entities)
        if (auditEntries.Any())
        {
            foreach (var auditEntry in auditEntries)
            {
                // For Added entities, get the ID now that it's been assigned
                if (auditEntry.Action == "Added" && auditEntry.EntityEntry != null)
                {
                    var keyName = auditEntry.EntityEntry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
                    if (keyName != null)
                    {
                        var keyValue = auditEntry.EntityEntry.Property(keyName).CurrentValue;
                        if (keyValue != null)
                        {
                            auditEntry.EntityId = keyValue.ToString();
                        }
                    }
                }
                
                var log = new Log
                {
                    TableName = auditEntry.TableName,
                    EntityId = int.TryParse(auditEntry.EntityId, out var entityId) ? entityId : 0,
                    Action = auditEntry.Action,
                    OldValues = auditEntry.OldValues,
                    NewValues = auditEntry.NewValues,
                    Changes = auditEntry.Changes,
                    UserId = auditEntry.UserId,
                    IpAddress = auditEntry.IpAddress,
                    UserAgent = auditEntry.UserAgent,
                    LoggedAt = DateTime.UtcNow
                };
                
                Logs.Add(log);
            }
            
            // Save audit logs
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private void UpdateTimestamps(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
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

    private AuditEntry CreateAuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string? userId, string? ipAddress, string? userAgent)
    {
        var auditEntry = new AuditEntry
        {
            TableName = entry.Entity.GetType().Name,
            Action = entry.State.ToString(),
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        // Get primary key value - for Added entities, we'll get this after SaveChanges
        var keyName = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
        if (keyName != null)
        {
            var keyValue = entry.Property(keyName).CurrentValue;
            if (keyValue != null && entry.State != EntityState.Added)
            {
                auditEntry.EntityId = keyValue.ToString();
            }
        }

        // Capture changes
        var changes = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;
            
            if (entry.State == EntityState.Added)
            {
                newValues[propertyName] = property.CurrentValue;
                if (property.CurrentValue != null)
                {
                    changes.Add($"{propertyName}: {property.CurrentValue}");
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (property.IsModified)
                {
                    oldValues[propertyName] = property.OriginalValue;
                    newValues[propertyName] = property.CurrentValue;
                    changes.Add($"{propertyName}: {property.OriginalValue} → {property.CurrentValue}");
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                oldValues[propertyName] = property.OriginalValue;
                if (property.OriginalValue != null)
                {
                    changes.Add($"{propertyName}: {property.OriginalValue}");
                }
            }
        }

        auditEntry.OldValues = oldValues.Any() ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null;
        auditEntry.NewValues = newValues.Any() ? System.Text.Json.JsonSerializer.Serialize(newValues) : null;
        auditEntry.Changes = string.Join("; ", changes);
        auditEntry.EntityEntry = entry;

        return auditEntry;
    }
}