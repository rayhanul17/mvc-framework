using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ModularHost.Web.Core.Services.Interfaces
{
    public interface IModuleDescriptor
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        string Version { get; }
        string Author { get; }
        string TablePrefix { get; }
        bool IsActive { get; set; }
        Type[] EntityTypes { get; }
        void ConfigureServices(IServiceCollection services);
        void Configure(IApplicationBuilder app);
    }
}