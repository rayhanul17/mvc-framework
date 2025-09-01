using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Services;
using MRCMS.Core.Services.Interfaces;

namespace MRCMS.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Automatically registers all services that inherit from BaseService
        /// </summary>
        public static IServiceCollection AddAutoServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                // Register all services that inherit from BaseService or BaseServiceWithDto
                var serviceTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition)
                    .Where(t => IsSubclassOfRawGeneric(typeof(BaseService<>), t) || 
                                IsSubclassOfRawGeneric(typeof(BaseServiceWithDto<,,,>), t));

                foreach (var serviceType in serviceTypes)
                {
                    // Find the interface that this service implements
                    var serviceInterface = serviceType.GetInterfaces()
                        .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");

                    if (serviceInterface != null)
                    {
                        services.AddScoped(serviceInterface, serviceType);
                    }
                    else
                    {
                        // If no interface found, register the concrete type
                        services.AddScoped(serviceType);
                    }
                }

                // Register all repositories
                var repositoryTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition)
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && 
                                i.GetGenericTypeDefinition() == typeof(IRepository<>)));

                foreach (var repositoryType in repositoryTypes)
                {
                    var repositoryInterface = repositoryType.GetInterfaces()
                        .FirstOrDefault(i => i.Name == $"I{repositoryType.Name}");

                    if (repositoryInterface != null)
                    {
                        services.AddScoped(repositoryInterface, repositoryType);
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Registers AutoMapper with all profiles from specified assemblies
        /// </summary>
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddAutoMapper(cfg => { }, assemblies);
            return services;
        }

        /// <summary>
        /// Registers a module with all its services
        /// </summary>
        public static IServiceCollection AddModule<TModule>(this IServiceCollection services) 
            where TModule : IModuleDescriptor, new()
        {
            var module = new TModule();
            
            // Register the module descriptor
            services.AddSingleton<IModuleDescriptor>(module);
            
            // Let the module configure its own services
            module.ConfigureServices(services);
            
            // Auto-register services from the module's assembly
            var moduleAssembly = typeof(TModule).Assembly;
            services.AddAutoServices(moduleAssembly);
            services.AddAutoMapperProfiles(moduleAssembly);
            
            return services;
        }

        /// <summary>
        /// Checks if a type is a subclass of a raw generic type
        /// </summary>
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType!;
            }
            return false;
        }

        /// <summary>
        /// Registers all modules found in specified assemblies
        /// </summary>
        public static IServiceCollection AddModules(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => typeof(IModuleDescriptor).IsAssignableFrom(t));

                foreach (var moduleType in moduleTypes)
                {
                    var module = Activator.CreateInstance(moduleType) as IModuleDescriptor;
                    if (module != null)
                    {
                        services.AddSingleton<IModuleDescriptor>(module);
                        module.ConfigureServices(services);
                        services.AddAutoServices(assembly);
                    }
                }
            }

            return services;
        }
    }
}