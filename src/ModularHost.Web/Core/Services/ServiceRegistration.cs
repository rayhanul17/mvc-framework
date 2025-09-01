using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MRCMS.Core.Services
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Automatically registers all services and repositories from the application and modules
        /// </summary>
        public static void RegisterDynamicServices(this IServiceCollection services)
        {
            // Register from main application
            var mainAssembly = Assembly.GetExecutingAssembly();
            RegisterServicesFromAssembly(services, mainAssembly);

            // Register from modules directory
            var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
            if (Directory.Exists(modulesPath))
            {
                var moduleFiles = Directory.GetFiles(modulesPath, "*.dll");
                foreach (var file in moduleFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        RegisterServicesFromAssembly(services, assembly);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load module for service registration {file}: {ex.Message}");
                    }
                }
            }

            // Register from the current assembly's modules folder structure
            RegisterServicesFromModulesFolder(services);
        }

        private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            // Register Services
            RegisterServiceTypes(services, types);

            // Register Repositories
            RegisterRepositoryTypes(services, types);
        }

        private static void RegisterServicesFromModulesFolder(IServiceCollection services)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            // Register Services from Modules folder structure
            var moduleServiceTypes = types.Where(t => 
                t.Namespace != null && 
                t.Namespace.Contains("Modules") && 
                (t.Namespace.Contains("Services") || t.Name.EndsWith("Service")))
                .ToList();

            foreach (var serviceType in moduleServiceTypes)
            {
                RegisterServiceType(services, serviceType);
            }

            // Register Repositories from Modules folder structure
            var moduleRepositoryTypes = types.Where(t => 
                t.Namespace != null && 
                t.Namespace.Contains("Modules") && 
                (t.Namespace.Contains("Repositories") || t.Name.EndsWith("Repository")))
                .ToList();

            foreach (var repositoryType in moduleRepositoryTypes)
            {
                RegisterRepositoryType(services, repositoryType);
            }
        }

        private static void RegisterServiceTypes(IServiceCollection services, List<Type> types)
        {
            // Method 1: Register by naming convention (ends with "Service")
            var serviceTypesByNaming = types
                .Where(t => t.Name.EndsWith("Service") && 
                           !t.Name.StartsWith("I") && 
                           !IsHostedService(t))
                .ToList();

            foreach (var serviceType in serviceTypesByNaming)
            {
                RegisterServiceType(services, serviceType);
            }

            // Method 2: Register by base class inheritance
            var baseServiceType = typeof(BaseService<>);
            var serviceTypesByInheritance = types
                .Where(t => IsSubclassOfGeneric(t, baseServiceType) && 
                           !serviceTypesByNaming.Contains(t))
                .ToList();

            foreach (var serviceType in serviceTypesByInheritance)
            {
                RegisterServiceType(services, serviceType);
            }
        }

        private static void RegisterRepositoryTypes(IServiceCollection services, List<Type> types)
        {
            // Method 1: Register by naming convention (ends with "Repository")
            var repositoryTypesByNaming = types
                .Where(t => t.Name.EndsWith("Repository") && !t.Name.StartsWith("I"))
                .ToList();

            foreach (var repositoryType in repositoryTypesByNaming)
            {
                RegisterRepositoryType(services, repositoryType);
            }

            // Method 2: Register by interface implementation
            var repositoryInterface = typeof(IRepository<>);
            var repositoryTypesByInterface = types
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == repositoryInterface) &&
                    !repositoryTypesByNaming.Contains(t))
                .ToList();

            foreach (var repositoryType in repositoryTypesByInterface)
            {
                RegisterRepositoryType(services, repositoryType);
            }
        }

        private static void RegisterServiceType(IServiceCollection services, Type serviceType)
        {
            // Skip if already registered
            if (IsAlreadyRegistered(services, serviceType))
                return;

            var interfaces = serviceType.GetInterfaces();
            
            // Look for matching interface (e.g., IUserService for UserService)
            var matchingInterface = interfaces.FirstOrDefault(i => 
                i.Name == $"I{serviceType.Name}");

            if (matchingInterface != null)
            {
                // Register with interface
                services.AddScoped(matchingInterface, serviceType);
                Console.WriteLine($"Registered service: {matchingInterface.Name} -> {serviceType.Name}");
            }
            else
            {
                // Register without interface
                services.AddScoped(serviceType);
                Console.WriteLine($"Registered service: {serviceType.Name}");
            }
        }

        private static void RegisterRepositoryType(IServiceCollection services, Type repositoryType)
        {
            // Skip if already registered
            if (IsAlreadyRegistered(services, repositoryType))
                return;

            var interfaces = repositoryType.GetInterfaces();
            
            // Look for matching interface
            var matchingInterface = interfaces.FirstOrDefault(i => 
                i.Name == $"I{repositoryType.Name}");

            if (matchingInterface != null)
            {
                // Register with interface
                services.AddScoped(matchingInterface, repositoryType);
                Console.WriteLine($"Registered repository: {matchingInterface.Name} -> {repositoryType.Name}");
            }
            else
            {
                // Check for generic repository interface
                var genericRepoInterface = interfaces.FirstOrDefault(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IRepository<>));

                if (genericRepoInterface != null)
                {
                    services.AddScoped(genericRepoInterface, repositoryType);
                    Console.WriteLine($"Registered repository: {genericRepoInterface.Name} -> {repositoryType.Name}");
                }
                else
                {
                    // Register without interface
                    services.AddScoped(repositoryType);
                    Console.WriteLine($"Registered repository: {repositoryType.Name}");
                }
            }
        }

        private static bool IsAlreadyRegistered(IServiceCollection services, Type type)
        {
            return services.Any(s => s.ServiceType == type || s.ImplementationType == type);
        }

        private static bool IsSubclassOfGeneric(Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                
                // Check if any base type matches
                if (toCheck.BaseType != null && toCheck.BaseType.IsGenericType)
                {
                    var baseGeneric = toCheck.BaseType.GetGenericTypeDefinition();
                    if (baseGeneric == generic)
                        return true;
                }
                
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private static bool IsHostedService(Type type)
        {
            return type.GetInterfaces().Any(i => 
                i.Name == "IHostedService" || 
                i.FullName?.Contains("Microsoft.Extensions.Hosting.IHostedService") == true);
        }

        /// <summary>
        /// Registers all services from specific folders
        /// </summary>
        public static void RegisterServicesFromFolders(this IServiceCollection services, params string[] folderPaths)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            foreach (var folderPath in folderPaths)
            {
                var normalizedPath = folderPath.Replace('/', '.').Replace('\\', '.');
                
                var servicesInFolder = types.Where(t => 
                    t.Namespace != null && 
                    t.Namespace.Contains(normalizedPath))
                    .ToList();

                foreach (var serviceType in servicesInFolder)
                {
                    if (serviceType.Name.EndsWith("Service"))
                    {
                        RegisterServiceType(services, serviceType);
                    }
                    else if (serviceType.Name.EndsWith("Repository"))
                    {
                        RegisterRepositoryType(services, serviceType);
                    }
                }
            }
        }
    }
}