using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Models.Entities;

namespace MRCMS.Core.Infrastructure
{
    /// <summary>
    /// Base class for module seeders with common functionality
    /// </summary>
    public abstract class ModuleSeederBase : IModuleSeeder
    {
        public abstract string ModuleName { get; }
        public virtual int Order => 100;

        protected ILogger? Logger { get; private set; }

        public async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            
            // Get logger
            var loggerFactory = scopedProvider.GetService<ILoggerFactory>();
            if (loggerFactory != null)
            {
                Logger = loggerFactory.CreateLogger(GetType());
            }

            Logger?.LogInformation($"Starting seed for module: {ModuleName}");

            try
            {
                // Get commonly used services
                var context = scopedProvider.GetRequiredService<AppDbContext>();
                var userManager = scopedProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scopedProvider.GetRequiredService<RoleManager<Role>>();

                // Execute module-specific seeding
                await SeedModuleDataAsync(scopedProvider, context, userManager, roleManager);

                // Save changes
                await context.SaveChangesAsync();

                Logger?.LogInformation($"Completed seed for module: {ModuleName}");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"Error seeding module: {ModuleName}");
                throw;
            }
        }

        public virtual async Task<bool> ShouldSeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check if module-specific table exists and has data
            return await CheckIfSeedingRequiredAsync(context);
        }

        /// <summary>
        /// Implement module-specific seeding logic
        /// </summary>
        protected abstract Task SeedModuleDataAsync(
            IServiceProvider serviceProvider, 
            AppDbContext context, 
            UserManager<User> userManager, 
            RoleManager<Role> roleManager);

        /// <summary>
        /// Check if seeding is required for this module
        /// </summary>
        protected abstract Task<bool> CheckIfSeedingRequiredAsync(AppDbContext context);

        /// <summary>
        /// Helper method to seed menus for the module
        /// </summary>
        protected async Task SeedModuleMenusAsync(AppDbContext context, Menu[] menus)
        {
            foreach (var menu in menus)
            {
                // Check if menu already exists
                var existingMenu = await context.Menus
                    .FirstOrDefaultAsync(m => m.Title == menu.Title && m.Url == menu.Url);

                if (existingMenu == null)
                {
                    menu.Id = Guid.NewGuid();
                    menu.CreatedAt = DateTime.UtcNow;
                    menu.UpdatedAt = DateTime.UtcNow;
                    menu.ModuleName = ModuleName;
                    
                    context.Menus.Add(menu);
                    Logger?.LogDebug($"Added menu: {menu.Title}");
                }
            }
        }

        /// <summary>
        /// Helper method to seed permissions for the module
        /// </summary>
        protected async Task SeedModulePermissionsAsync(AppDbContext context, string roleName, RolePermission[] permissions)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                Logger?.LogWarning($"Role {roleName} not found for permissions seeding");
                return;
            }

            foreach (var permission in permissions)
            {
                // Check if permission already exists
                var existingPermission = await context.RolePermissions
                    .FirstOrDefaultAsync(p => p.RoleId == role.Id && 
                                             p.Url == permission.Url && 
                                             p.HttpMethod == permission.HttpMethod);

                if (existingPermission == null)
                {
                    permission.Id = Guid.NewGuid();
                    permission.RoleId = role.Id;
                    permission.CreatedAt = DateTime.UtcNow;
                    
                    context.RolePermissions.Add(permission);
                    Logger?.LogDebug($"Added permission: {permission.Url} ({permission.HttpMethod}) for role {roleName}");
                }
            }
        }

        /// <summary>
        /// Helper method to get or create a parent menu
        /// </summary>
        protected async Task<Menu?> GetOrCreateParentMenuAsync(AppDbContext context, string title, string url, string icon, int order)
        {
            var menu = await context.Menus.FirstOrDefaultAsync(m => m.Title == title);
            
            if (menu == null)
            {
                menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Url = url,
                    Icon = icon,
                    Order = order,
                    IsVisible = true,
                    ModuleName = ModuleName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.Menus.Add(menu);
                await context.SaveChangesAsync();
            }
            
            return menu;
        }
    }
}