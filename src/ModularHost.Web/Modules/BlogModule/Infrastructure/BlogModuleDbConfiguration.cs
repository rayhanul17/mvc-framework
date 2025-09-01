using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Modules.BlogModule.Infrastructure.Configurations;

namespace MRCMS.Modules.BlogModule.Infrastructure
{
    /// <summary>
    /// Blog module database configuration
    /// </summary>
    public class BlogModuleDbConfiguration : IModuleDbConfiguration
    {
        public string ModuleName => "BlogModule";

        public void Configure(ModelBuilder modelBuilder)
        {
            // Apply all entity configurations for the blog module
            modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new CommentConfiguration());
            modelBuilder.ApplyConfiguration(new BlogPostTagConfiguration());
        }
    }
}