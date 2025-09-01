using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MRCMS.Core.Infrastructure
{
    /// <summary>
    /// Interface for module-specific data seeding
    /// </summary>
    public interface IModuleSeeder
    {
        /// <summary>
        /// Gets the module name
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Gets the execution order for seeding (lower numbers execute first)
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Seeds the module-specific data
        /// </summary>
        /// <param name="serviceProvider">Service provider for dependency resolution</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SeedAsync(IServiceProvider serviceProvider);

        /// <summary>
        /// Checks if seeding is required
        /// </summary>
        /// <param name="serviceProvider">Service provider for dependency resolution</param>
        /// <returns>True if seeding should be performed, false otherwise</returns>
        Task<bool> ShouldSeedAsync(IServiceProvider serviceProvider);
    }
}