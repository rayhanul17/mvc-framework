using Microsoft.Extensions.DependencyInjection;
using Nexora.Infrastructure.Data.SeedData;

namespace Nexora.Infrastructure.Data;

/// <summary>
/// Legacy DbInitializer maintained for backward compatibility.
/// Delegates to the new modular seeding system.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database with seed data.
    /// This method is maintained for backward compatibility.
    /// New code should use DatabaseSeeder directly.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Delegate to the new seeding system
        await DatabaseSeeder.SeedAsync(serviceProvider);
    }
}