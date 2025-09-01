using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Models.Entities;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Core.Constants;
using MRCMS.Core.Infrastructure.Configurations;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MRCMS.Core.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid, 
        IdentityUserClaim<Guid>, MRCMS.Core.Models.Entities.UserRole, IdentityUserLogin<Guid>,
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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        
        // Blog Module entities
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply core entity configurations
            ApplyCoreConfigurations(modelBuilder);

            // Configure Identity tables that don't have separate configuration files
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable(TableNames.UserClaims);
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable(TableNames.UserLogins);
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable(TableNames.UserTokens);
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable(TableNames.RoleClaims);

            // Apply module configurations dynamically
            ApplyModuleConfigurations(modelBuilder);
        }

        private void ApplyCoreConfigurations(ModelBuilder modelBuilder)
        {
            // Apply all core entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new MenuConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new LogConfiguration());
            modelBuilder.ApplyConfiguration(new LogArchiveConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new SettingConfiguration());
        }

        private void ApplyModuleConfigurations(ModelBuilder modelBuilder)
        {
            // Find and apply module configurations from the current assembly first
            var moduleConfigTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IModuleDbConfiguration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var configType in moduleConfigTypes)
            {
                try
                {
                    var config = Activator.CreateInstance(configType) as IModuleDbConfiguration;
                    config?.Configure(modelBuilder);
                    Console.WriteLine($"Applied configuration from module: {config?.ModuleName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to apply configuration from {configType.Name}: {ex.Message}");
                }
            }

            // Also load configurations from external module assemblies
            var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
            if (!Directory.Exists(modulesPath)) return;

            foreach (var file in Directory.GetFiles(modulesPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    
                    // Find IModuleDbConfiguration implementations
                    var externalConfigTypes = assembly.GetTypes()
                        .Where(t => typeof(IModuleDbConfiguration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .ToList();

                    foreach (var configType in externalConfigTypes)
                    {
                        try
                        {
                            var config = Activator.CreateInstance(configType) as IModuleDbConfiguration;
                            config?.Configure(modelBuilder);
                            Console.WriteLine($"Applied configuration from external module: {config?.ModuleName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to apply configuration from {configType.Name}: {ex.Message}");
                        }
                    }

                    // Also apply any IEntityTypeConfiguration implementations directly
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