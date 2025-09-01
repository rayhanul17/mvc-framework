using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Services;
using MRCMS.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class ModuleLoader
    {
        public static void LoadModules(IServiceCollection services, ApplicationPartManager partManager)
        {
            // First, scan for modules in the current assembly (embedded modules)
            var currentAssembly = Assembly.GetExecutingAssembly();
            RegisterModuleServices(services, currentAssembly);
            CallModuleInitializer(services, currentAssembly);
            Console.WriteLine($"Scanned current assembly for modules: {currentAssembly.GetName().Name}");

            // Then, scan for external module DLLs
            var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
            
            if (!Directory.Exists(modulesPath))
            {
                Directory.CreateDirectory(modulesPath);
                return;
            }

            var moduleFiles = Directory.GetFiles(modulesPath, "*.dll");
            
            foreach (var file in moduleFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    
                    // Add assembly as application part for MVC discovery
                    partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                    
                    // Register services from the module using dynamic discovery
                    RegisterModuleServices(services, assembly);
                    
                    // Call module initializer if exists
                    CallModuleInitializer(services, assembly);
                    
                    Console.WriteLine($"Loaded external module: {assembly.GetName().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load module {file}: {ex.Message}");
                }
            }
        }

        private static void RegisterModuleServices(IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            // Track what we register to avoid duplicates
            var registeredTypes = new HashSet<Type>();

            foreach (var type in types)
            {
                // Skip if already registered
                if (registeredTypes.Contains(type))
                    continue;

                // Skip controllers - they are automatically registered by MVC
                if (type.Name.EndsWith("Controller"))
                    continue;

                // Method 1: Register by naming convention
                if (type.Name.EndsWith("Service") && !type.Name.StartsWith("I"))
                {
                    RegisterService(services, type);
                    registeredTypes.Add(type);
                }
                else if (type.Name.EndsWith("Repository") && !type.Name.StartsWith("I"))
                {
                    RegisterRepository(services, type);
                    registeredTypes.Add(type);
                }
                // Method 2: Register by base class inheritance
                else if (IsServiceByInheritance(type))
                {
                    RegisterService(services, type);
                    registeredTypes.Add(type);
                }
                else if (IsRepositoryByInheritance(type))
                {
                    RegisterRepository(services, type);
                    registeredTypes.Add(type);
                }
            }
        }

        private static bool IsServiceByInheritance(Type type)
        {
            // Check if inherits from BaseService<T>
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && 
                    baseType.GetGenericTypeDefinition() == typeof(BaseService<>))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private static bool IsRepositoryByInheritance(Type type)
        {
            // Check if implements IRepository<T>
            return type.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRepository<>));
        }

        private static void RegisterService(IServiceCollection services, Type serviceType)
        {
            // Check if already registered
            if (services.Any(s => s.ServiceType == serviceType || s.ImplementationType == serviceType))
            {
                Console.WriteLine($"Service {serviceType.Name} already registered, skipping.");
                return;
            }

            var interfaces = serviceType.GetInterfaces();
            
            // Look for matching interface (e.g., IUserService for UserService)
            var matchingInterface = interfaces.FirstOrDefault(i => 
                i.Name == $"I{serviceType.Name}");

            if (matchingInterface != null)
            {
                services.AddScoped(matchingInterface, serviceType);
                Console.WriteLine($"Registered service: {matchingInterface.Name} -> {serviceType.Name}");
            }
            else
            {
                // Try to find any service interface
                var serviceInterface = interfaces.FirstOrDefault(i => 
                    i.Name.EndsWith("Service") && i.Name.StartsWith("I"));
                
                if (serviceInterface != null)
                {
                    services.AddScoped(serviceInterface, serviceType);
                    Console.WriteLine($"Registered service: {serviceInterface.Name} -> {serviceType.Name}");
                }
                else
                {
                    services.AddScoped(serviceType);
                    Console.WriteLine($"Registered service: {serviceType.Name} (no interface)");
                }
            }
        }

        private static void RegisterRepository(IServiceCollection services, Type repositoryType)
        {
            // Check if already registered
            if (services.Any(s => s.ServiceType == repositoryType || s.ImplementationType == repositoryType))
            {
                Console.WriteLine($"Repository {repositoryType.Name} already registered, skipping.");
                return;
            }

            var interfaces = repositoryType.GetInterfaces();
            
            // Look for matching interface
            var matchingInterface = interfaces.FirstOrDefault(i => 
                i.Name == $"I{repositoryType.Name}");

            if (matchingInterface != null)
            {
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
                    services.AddScoped(repositoryType);
                    Console.WriteLine($"Registered repository: {repositoryType.Name} (no interface)");
                }
            }
        }

        private static void CallModuleInitializer(IServiceCollection services, Assembly assembly)
        {
            // Register module seeders
            RegisterModuleSeeders(services, assembly);
            
            // Call module initializer if exists
            var initializerType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "ModuleInitializer" || t.Name == "ModuleStartup");
            
            if (initializerType != null)
            {
                var initMethod = initializerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
                if (initMethod != null)
                {
                    initMethod.Invoke(null, new object[] { services });
                }
                else
                {
                    // Try instance method
                    var instance = Activator.CreateInstance(initializerType);
                    initMethod = initializerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                    initMethod?.Invoke(instance, new object[] { services });
                }
            }
        }

        private static void RegisterModuleSeeders(IServiceCollection services, Assembly assembly)
        {
            var seederInterface = typeof(IModuleSeeder);
            var seederTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && seederInterface.IsAssignableFrom(t))
                .ToList();

            foreach (var seederType in seederTypes)
            {
                services.AddTransient(seederInterface, seederType);
                Console.WriteLine($"Registered module seeder: {seederType.Name}");
            }
        }

        /// <summary>
        /// Execute all registered module seeders
        /// </summary>
        public static async Task ExecuteModuleSeedersAsync(IServiceProvider serviceProvider)
        {
            var seeders = serviceProvider.GetServices<IModuleSeeder>()
                .OrderBy(s => s.Order)
                .ToList();

            foreach (var seeder in seeders)
            {
                try
                {
                    if (await seeder.ShouldSeedAsync(serviceProvider))
                    {
                        Console.WriteLine($"Executing seeder for module: {seeder.ModuleName}");
                        await seeder.SeedAsync(serviceProvider);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping seeder for module: {seeder.ModuleName} (already seeded)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing seeder for module {seeder.ModuleName}: {ex.Message}");
                }
            }
        }
    }
}