using Microsoft.Extensions.DependencyInjection;
using mvc.framework.Data;
using mvc.framework.Services;

namespace mvc.framework.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGenericRepository<T>(this IServiceCollection services) where T : class
        {
            services.AddScoped<IGenericRepository<T>, GenericRepository<T>>();
            return services;
        }

        public static IServiceCollection AddGenericService<T>(this IServiceCollection services) where T : class
        {
            services.AddScoped<IGenericRepository<T>, GenericRepository<T>>();
            services.AddScoped<IGenericService<T>, GenericService<T>>();
            return services;
        }

        public static IServiceCollection AddGenericServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
            return services;
        }
    }
}