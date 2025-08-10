using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;
using Nexora.Infrastructure.Data;
using Nexora.Infrastructure.Repositories;

namespace Nexora.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
                mySqlOptions => mySqlOptions.MigrationsAssembly("Nexora.Web")
            ));

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