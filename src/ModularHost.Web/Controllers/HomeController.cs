using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Models.ViewModels;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Models.DTOs;
using AutoMapper;

namespace MRCMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<BlogPost> _blogRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Tag> _tagRepository;
    private readonly IMapper _mapper;

    public HomeController(
        ILogger<HomeController> logger,
        IRepository<BlogPost> blogRepository,
        IRepository<Category> categoryRepository,
        IRepository<Tag> tagRepository,
        IMapper mapper)
    {
        _logger = logger;
        _blogRepository = blogRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(string? category = null, string? tag = null, string? search = null, int page = 1)
    {
        const int pageSize = 9;

        var query = _blogRepository.Query()
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.BlogPostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsPublished && !p.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Name == category);
        }

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(p => p.BlogPostTags.Any(pt => pt.Tag.Name == tag));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => 
                p.Title.Contains(search) ||
                p.Summary.Contains(search) ||
                p.Content.Contains(search));
        }

        // Get total count for pagination
        var totalPosts = await query.CountAsync();

        // Get posts for current page
        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get categories and tags for filtering
        var categories = await _categoryRepository.Query()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var tags = await _tagRepository.Query()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var postDtos = _mapper.Map<List<BlogPostDto>>(posts);

        var viewModel = new LandingPageViewModel
        {
            Posts = postDtos,
            Categories = categories,
            Tags = tags,
            CurrentCategory = category,
            CurrentTag = tag,
            SearchQuery = search,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPosts = totalPosts,
            TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize)
        };

        return View(viewModel);
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
