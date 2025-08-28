using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModularHost.Web.Core.Models.Entities;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Core.Constants;
using ModularHost.Web.Modules.Blog.Constants;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ModularHost.Web.Core.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid, 
        IdentityUserClaim<Guid>, ModularHost.Web.Core.Models.Entities.UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Users and Roles are already defined in IdentityDbContext
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogArchive> LogArchives { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        
        // Blog Module entities
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity tables with custom fields
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.Users);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.ProfilePicture).HasMaxLength(500);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.Roles);
                entity.Property(e => e.Description).HasMaxLength(255);
            });

            modelBuilder.Entity<ModularHost.Web.Core.Models.Entities.UserRole>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.UserRoles);
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                
                // Configure relationships explicitly to ensure proper loading
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                    
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            // Configure other Identity tables
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable(ModularHost.Web.Core.Constants.TableNames.UserClaims);
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable(ModularHost.Web.Core.Constants.TableNames.UserLogins);
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable(ModularHost.Web.Core.Constants.TableNames.UserTokens);
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable(ModularHost.Web.Core.Constants.TableNames.RoleClaims);

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.Menus);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Url).HasMaxLength(255);
                entity.Property(e => e.Icon).HasMaxLength(50);
                entity.Property(e => e.ClaimType).HasMaxLength(100);
                entity.HasOne(e => e.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.RolePermissions);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RoleId, e.Url, e.HttpMethod }).IsUnique();
                entity.Property(e => e.Url).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.Logs);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.Level).HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(4000);
                entity.Property(e => e.Exception).HasColumnType("TEXT");
                entity.Property(e => e.Properties).HasColumnType("TEXT");
            });

            // LogArchive inherits from Log, so it doesn't need separate key configuration
            modelBuilder.Entity<LogArchive>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.LogArchives);
                entity.HasIndex(e => e.ArchivedAt);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable(ModularHost.Web.Core.Constants.TableNames.Notifications);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).HasColumnType("TEXT");
                entity.Property(e => e.TargetRoles).HasMaxLength(500);
                entity.Property(e => e.TargetUsers).HasMaxLength(500);
                entity.Property(e => e.ReadBy).HasColumnType("TEXT");
            });

            // Blog Module entity configurations
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable(ModularHost.Web.Modules.Blog.Constants.TableNames.BlogPosts);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Summary).HasMaxLength(500);
                entity.Property(e => e.FeaturedImage).HasMaxLength(500);
                entity.Property(e => e.MetaTitle).HasMaxLength(255);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);
                entity.Property(e => e.MetaKeywords).HasMaxLength(500);
                // Status property removed - using IsPublished instead
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.PublishedAt);
                
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.BlogPosts)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable(ModularHost.Web.Modules.Blog.Constants.TableNames.Categories);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable(ModularHost.Web.Modules.Blog.Constants.TableNames.Tags);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable(ModularHost.Web.Modules.Blog.Constants.TableNames.Comments);
                entity.Property(e => e.Body).IsRequired().HasColumnType("TEXT");
                
                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BlogPostTag>(entity =>
            {
                entity.ToTable(ModularHost.Web.Modules.Blog.Constants.TableNames.BlogPostTags);
                entity.HasKey(e => new { e.BlogPostId, e.TagId });
                
                entity.HasOne(e => e.BlogPost)
                    .WithMany(p => p.BlogPostTags)
                    .HasForeignKey(e => e.BlogPostId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.BlogPostTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Apply configurations from modules
            ApplyModuleConfigurations(modelBuilder);
        }

        private void ApplyModuleConfigurations(ModelBuilder modelBuilder)
        {
            var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
            if (!Directory.Exists(modulesPath)) return;

            foreach (var file in Directory.GetFiles(modulesPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    modelBuilder.ApplyConfigurationsFromAssembly(assembly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load module configurations from {file}: {ex.Message}");
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = GetCurrentUser();
                }

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = GetCurrentUser();
            }
        }

        private Guid? GetCurrentUser()
        {
            // This will be implemented to get the current user from HttpContext
            // For now, return null for anonymous actions
            return null;
        }
    }
}