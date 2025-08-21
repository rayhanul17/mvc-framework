using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class BlogCategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.BlogCategories.AnyAsync())
            return;

        var categories = new List<BlogCategory>
        {
            new BlogCategory
            {
                Name = "কুরআন ও তাফসীর",
                Slug = "quran-tafseer",
                Description = "পবিত্র কুরআনের তাফসীর, ব্যাখ্যা এবং শিক্ষা সম্পর্কিত আর্টিকেল",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "হাদীস শরীফ",
                Slug = "hadith-sharif",
                Description = "রাসূলুল্লাহ (সা:) এর হাদীস এবং সুন্নাহ সম্পর্কিত পোস্ট",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "ইসলামিক ইতিহাস",
                Slug = "islamic-history",
                Description = "ইসলামের গৌরবময় ইতিহাস ও মুসলিম বীরদের কাহিনী",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "ফিকহ ও মাসআলা",
                Slug = "fiqh-masala",
                Description = "দৈনন্দিন জীবনের ইসলামিক বিধিবিধান ও সমাধান",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "দুআ ও যিকির",
                Slug = "dua-zikir",
                Description = "মাসনূন দুআ, যিকির এবং আমলের ফযীলত",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "নামাজ ও ইবাদত",
                Slug = "namaz-ibadat",
                Description = "নামাজ, রোজা, হজ্জ, যাকাত সহ সকল ইবাদতের নিয়মকানুন",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "ইসলামিক গল্প",
                Slug = "islamic-stories",
                Description = "নবী-রাসূল ও সাহাবীদের জীবনী এবং শিক্ষণীয় ঘটনা",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BlogCategory
            {
                Name = "রমজান বিশেষ",
                Slug = "ramadan-special",
                Description = "পবিত্র রমজান মাসের বিশেষ আমল ও করণীয়",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.BlogCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}