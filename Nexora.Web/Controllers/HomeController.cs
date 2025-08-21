using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using Nexora.Web.Models;
using Nexora.Web.Models.ViewModels;
using System.Security.Claims;

namespace Nexora.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISiteSettingService _siteSettingService;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<ApplicationUser> userManager,
        ISiteSettingService siteSettingService,
        ApplicationDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _siteSettingService = siteSettingService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Fix Dashboard menu if needed (temporary fix)
        var dashboardMenu = await _context.Menus.FirstOrDefaultAsync(m => m.Name == "Dashboard" && m.Controller == "Home");
        if (dashboardMenu != null && dashboardMenu.Action == "Index")
        {
            dashboardMenu.Action = "Dashboard";
            await _context.SaveChangesAsync();
        }
        
        // Don't redirect authenticated users - show them the blog landing page too
        var model = new BlogLandingViewModel();
        
        // Get featured posts (popular recent posts - balancing recency and views)
        model.FeaturedPosts = await _context.BlogPosts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .ThenByDescending(p => p.PublishedDate)
            .Take(3)
            .Select(p => new BlogPostSummary
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary ?? "",
                FeaturedImageUrl = p.FeaturedImageUrl ?? "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=800&h=400&fit=crop",
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug ?? "",
                AuthorName = p.Author != null ? p.Author.FullName : "Anonymous",
                PublishedDate = p.PublishedDate ?? p.CreatedAt,
                ViewCount = p.ViewCount,
                Tags = new List<string>()
            })
            .ToListAsync();
        
        // Get recent posts
        model.RecentPosts = await _context.BlogPosts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .Skip(3)
            .Take(6)
            .Select(p => new BlogPostSummary
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary ?? "",
                FeaturedImageUrl = p.FeaturedImageUrl ?? "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=800&h=400&fit=crop",
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug ?? "",
                AuthorName = p.Author != null ? p.Author.FullName : "Anonymous",
                PublishedDate = p.PublishedDate ?? p.CreatedAt,
                ViewCount = p.ViewCount,
                Tags = new List<string>()
            })
            .ToListAsync();
        
        // Get popular posts (by view count)
        model.PopularPosts = await _context.BlogPosts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .Select(p => new BlogPostSummary
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Summary = p.Summary ?? "",
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug ?? "",
                AuthorName = p.Author != null ? p.Author.FullName : "Anonymous",
                PublishedDate = p.PublishedDate ?? p.CreatedAt,
                ViewCount = p.ViewCount
            })
            .ToListAsync();
        
        // Get categories with post counts
        model.Categories = await _context.BlogCategories
            .Include(c => c.BlogPosts)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryInfo
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug ?? "",
                Description = c.Description ?? "",
                PostCount = c.BlogPosts.Count(p => p.IsPublished),
                DisplayOrder = c.DisplayOrder
            })
            .ToListAsync();
        
        // Get all unique tags from published posts
        var allTags = await _context.BlogPosts
            .Where(p => p.IsPublished && !string.IsNullOrEmpty(p.Tags))
            .Select(p => p.Tags)
            .ToListAsync();
        
        var tagCounts = new Dictionary<string, int>();
        foreach (var tagString in allTags)
        {
            if (!string.IsNullOrEmpty(tagString))
            {
                var tags = tagString.Split(',').Select(t => t.Trim());
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (tagCounts.ContainsKey(tag))
                            tagCounts[tag]++;
                        else
                            tagCounts[tag] = 1;
                    }
                }
            }
        }
        
        model.PopularTags = tagCounts
            .OrderByDescending(t => t.Value)
            .Take(15)
            .Select(t => new TagInfo { Name = t.Key, Count = t.Value })
            .ToList();
        
        // Get site settings
        var siteSettings = await _siteSettingService.GetAllSettingsAsync();
        if (siteSettings.IsSuccess && siteSettings.Data != null)
        {
            model.SiteName = siteSettings.Data.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? "Nexora Blog";
            model.SiteDescription = siteSettings.Data.FirstOrDefault(s => s.Key == "SiteDescription")?.Value ?? "Explore insights, tutorials, and industry trends";
        }
        
        return View(model);
    }

    public IActionResult NotificationDemo()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        return await RedirectToDashboard();
    }

    private async Task<IActionResult> RedirectToDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Check for Super Admin or Administrator first
            if (userRoles.Contains("SuperAdmin") || userRoles.Contains("Administrator"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            
            // Check for Customer Support roles
            if (userRoles.Contains("CustomerSupportAdmin") || 
                userRoles.Contains("CustomerSupportManager") || 
                userRoles.Contains("CustomerSupportAgent"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "CustomerSupport" });
            }
            
            // Check for Customer Service roles (role mapping system)
            if (userRoles.Any(r => r.StartsWith("CustomerService")))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "CustomerService" });
            }
            
            // Default dashboard for regular users
            return RedirectToAction("UserDashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining dashboard redirect for user");
            return RedirectToAction("UserDashboard");
        }
    }

    [Authorize]
    public async Task<IActionResult> UserDashboard()
    {
        var model = await BuildUserDashboardModel();
        return View(model);
    }

    private async Task<UserDashboardViewModel> BuildUserDashboardModel()
    {
        var model = new UserDashboardViewModel();
        
        // Get site settings
        var siteSettingsResult = await _siteSettingService.GetAllSettingsAsync();
        if (siteSettingsResult.IsSuccess && siteSettingsResult.Data != null)
        {
            model.SiteName = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? "Nexora Framework";
            model.SiteSlogan = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "SiteSlogan")?.Value ?? "Dynamic Role-Based System";
            model.WelcomeMessage = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "UserWelcomeMessage")?.Value ?? "Welcome to your dashboard";
        }

        // Get user information
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                model.UserName = user.UserName ?? "";
                model.FullName = user.FullName;
                model.Email = user.Email ?? "";
                model.UserRoles = (await _userManager.GetRolesAsync(user)).ToList();
                model.MemberSince = user.CreatedAt;
            }
        }

        return model;
    }

    [AllowAnonymous]
    public IActionResult Category(string slug)
    {
        // The Filter view will handle everything via AJAX
        return View("Filter");
    }

    [AllowAnonymous]
    public IActionResult Tag(string tag)
    {
        // The Filter view will handle everything via AJAX
        return View("Filter");
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GetPostsByCategory(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return Json(new { success = false, message = "Category slug is required" });
        }

        var category = await _context.BlogCategories
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        if (category == null)
        {
            return Json(new { success = false, message = "Category not found" });
        }

        var posts = await _context.BlogPosts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.CategoryId == category.Id && p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug ?? "",
                summary = p.Summary ?? "",
                featuredImageUrl = p.FeaturedImageUrl ?? "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=800&h=400&fit=crop",
                categoryName = p.Category != null ? p.Category.Name : "",
                categorySlug = p.Category != null ? p.Category.Slug ?? "" : "",
                authorName = p.Author != null ? p.Author.FullName : "Anonymous",
                publishedDate = p.PublishedDate ?? p.CreatedAt,
                viewCount = p.ViewCount,
                tags = string.IsNullOrEmpty(p.Tags) ? new List<string>() : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
            })
            .ToListAsync();

        return Json(new { success = true, posts = posts, categoryName = category.Name });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GetPostsByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return Json(new { success = false, message = "Tag is required" });
        }

        // Decode URL encoded characters
        tag = System.Net.WebUtility.UrlDecode(tag);

        var posts = await _context.BlogPosts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsPublished && p.Tags != null && p.Tags.Contains(tag))
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug ?? "",
                summary = p.Summary ?? "",
                featuredImageUrl = p.FeaturedImageUrl ?? "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=800&h=400&fit=crop",
                categoryName = p.Category != null ? p.Category.Name : "",
                categorySlug = p.Category != null ? p.Category.Slug ?? "" : "",
                authorName = p.Author != null ? p.Author.FullName : "Anonymous",
                publishedDate = p.PublishedDate ?? p.CreatedAt,
                viewCount = p.ViewCount,
                tags = string.IsNullOrEmpty(p.Tags) ? new List<string>() : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
            })
            .ToListAsync();

        return Json(new { success = true, posts = posts, tagName = tag });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetSidebarData()
    {
        // Get categories
        var categories = await _context.BlogCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                name = c.Name,
                slug = c.Slug ?? "",
                postCount = _context.BlogPosts.Count(p => p.CategoryId == c.Id && p.IsPublished)
            })
            .ToListAsync();

        // Get popular tags
        var allTags = await _context.BlogPosts
            .Where(p => p.IsPublished && p.Tags != null && p.Tags != "")
            .Select(p => p.Tags)
            .ToListAsync();

        var tagCounts = new Dictionary<string, int>();
        foreach (var tagString in allTags)
        {
            if (!string.IsNullOrEmpty(tagString))
            {
                var tags = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in tags)
                {
                    var trimmedTag = tag.Trim();
                    if (!string.IsNullOrEmpty(trimmedTag))
                    {
                        if (tagCounts.ContainsKey(trimmedTag))
                            tagCounts[trimmedTag]++;
                        else
                            tagCounts[trimmedTag] = 1;
                    }
                }
            }
        }

        var popularTags = tagCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(15)
            .Select(kvp => new { name = kvp.Key, count = kvp.Value })
            .ToList();

        // Get popular posts
        var popularPosts = await _context.BlogPosts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug ?? "",
                publishedDate = p.PublishedDate ?? p.CreatedAt,
                viewCount = p.ViewCount
            })
            .ToListAsync();

        return Json(new
        {
            categories = categories,
            popularTags = popularTags,
            popularPosts = popularPosts
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
