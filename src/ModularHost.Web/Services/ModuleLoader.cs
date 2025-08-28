using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModularHost.Web.Services
{
    public class ModuleLoader
    {
        public static void LoadModules(IServiceCollection services, ApplicationPartManager partManager)
        {
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
                    
                    // Register services from the module
                    RegisterModuleServices(services, assembly);
                    
                    // Call module initializer if exists
                    CallModuleInitializer(services, assembly);
                    
                    Console.WriteLine($"Loaded module: {assembly.GetName().Name}");
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
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType);

            foreach (var type in types)
            {
                // Register controllers
                if (type.Name.EndsWith("Controller"))
                {
                    // Controllers are automatically registered by MVC
                    continue;
                }
                
                // Register services
                if (type.Name.EndsWith("Service"))
                {
                    var interfaces = type.GetInterfaces();
                    if (interfaces.Any())
                    {
                        var serviceInterface = interfaces.FirstOrDefault(i => i.Name == $"I{type.Name}");
                        if (serviceInterface != null)
                        {
                            services.AddScoped(serviceInterface, type);
                        }
                        else
                        {
                            services.AddScoped(type);
                        }
                    }
                    else
                    {
                        services.AddScoped(type);
                    }
                }
                
                // Register repositories
                if (type.Name.EndsWith("Repository"))
                {
                    var interfaces = type.GetInterfaces();
                    if (interfaces.Any())
                    {
                        var repositoryInterface = interfaces.FirstOrDefault(i => i.Name == $"I{type.Name}");
                        if (repositoryInterface != null)
                        {
                            services.AddScoped(repositoryInterface, type);
                        }
                        else
                        {
                            services.AddScoped(type);
                        }
                    }
                    else
                    {
                        services.AddScoped(type);
                    }
                }
            }
        }

        private static void CallModuleInitializer(IServiceCollection services, Assembly assembly)
        {
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
    }
}