using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;
using Nexora.Infrastructure.Data;
using Nexora.Infrastructure.Repositories;

namespace Nexora.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Get environment to determine if we're in development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == Environments.Development;
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
                mySqlOptions => mySqlOptions.MigrationsAssembly("Nexora.Web")
            );
            
            // Enable detailed logging in development
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging() // Shows parameter values in logs
                       .EnableDetailedErrors() // Shows detailed error information
                       .LogTo(Console.WriteLine, new[] {
                           DbLoggerCategory.Database.Command.Name,
                           DbLoggerCategory.Query.Name
                       }, LogLevel.Information); // Log SQL queries to console
            }
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Nexora.Application");
        
        var serviceTypes = applicationAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Service") && !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");
            
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, serviceType);
            }
            else
            {
                services.AddScoped(serviceType);
            }
        }

        var repositoryTypes = applicationAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Repository") && !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
            .ToList();

        foreach (var repositoryType in repositoryTypes)
        {
            var interfaceType = repositoryType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{repositoryType.Name}");
            
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, repositoryType);
            }
            else
            {
                services.AddScoped(repositoryType);
            }
        }

        return services;
    }
}