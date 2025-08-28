using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Modules.Blog.Models.Entities;
using ModularHost.Web.Modules.Blog.Services;
using ModularHost.Web.Modules.Blog.Services.Interfaces;

namespace BlogModule
{
    public class BlogModuleDescriptor : IModuleDescriptor
    {
        public string Name => "BlogModule";
        public string DisplayName => "Blog Module";
        public string Description => "A comprehensive blogging module with categories, tags, and comments";
        public string Version => "1.0.0";
        public string Author => "ModularHost Team";
        public string TablePrefix => "blog";
        public bool IsActive { get; set; } = true;
        
        public Type[] EntityTypes => new[]
        {
            typeof(BlogPost),
            typeof(Category),
            typeof(Tag),
            typeof(BlogPostTag),
            typeof(Comment)
        };

        public void ConfigureServices(IServiceCollection services)
        {
            // Register module-specific services
            services.AddScoped<IBlogPostService, BlogPostService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICommentService, CommentService>();
            
            // Register the old BlogService for backward compatibility
            services.AddScoped<BlogService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Module-specific middleware or configuration
            // This could be used to set up module-specific routes, middleware, etc.
        }
    }
}