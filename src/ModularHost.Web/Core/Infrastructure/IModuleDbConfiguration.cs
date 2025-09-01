using Microsoft.EntityFrameworkCore;

namespace MRCMS.Core.Infrastructure
{
    /// <summary>
    /// Interface for module database configuration
    /// Each module should implement this to configure its entities
    /// </summary>
    public interface IModuleDbConfiguration
    {
        /// <summary>
        /// Apply entity configurations to the model builder
        /// </summary>
        void Configure(ModelBuilder modelBuilder);
        
        /// <summary>
        /// Get the module name
        /// </summary>
        string ModuleName { get; }
    }
}