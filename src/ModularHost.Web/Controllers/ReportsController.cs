using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Services.Interfaces;
using MRCMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuditLogger _auditLogger;

        public ReportsController(
            AppDbContext context,
            UserManager<User> userManager,
            IAuditLogger auditLogger)
        {
            _context = context;
            _userManager = userManager;
            _auditLogger = auditLogger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserActivity(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var viewModel = new UserActivityReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync(),
                NewUsersInPeriod = await _userManager.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .CountAsync(),
                UsersByRole = await GetUsersByRoleAsync(),
                DailyRegistrations = await GetDailyRegistrationsAsync(startDate.Value, endDate.Value),
                TopActiveUsers = await GetTopActiveUsersAsync(startDate.Value, endDate.Value),
                UserGrowthRate = await CalculateUserGrowthRateAsync(startDate.Value, endDate.Value)
            };

            // Get login statistics
            var loginStats = await _auditLogger.GetLoginStatisticsAsync(startDate.Value, endDate.Value);
            viewModel.TotalLogins = loginStats.TotalLogins;
            viewModel.UniqueUserLogins = loginStats.UniqueUsers;
            viewModel.FailedLoginAttempts = loginStats.FailedAttempts;

            return View(viewModel);
        }

        public async Task<IActionResult> BlogStats(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var viewModel = new BlogStatsReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalPosts = await _context.Set<BlogPost>().CountAsync(),
                PublishedPosts = await _context.Set<BlogPost>()
                    .Where(p => p.IsPublished)
                    .CountAsync(),
                DraftPosts = await _context.Set<BlogPost>()
                    .Where(p => !p.IsPublished)
                    .CountAsync(),
                TotalViews = await _context.Set<BlogPost>()
                    .SumAsync(p => p.ViewCount),
                TotalComments = await _context.Set<Comment>().CountAsync(),
                PostsInPeriod = await _context.Set<BlogPost>()
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .CountAsync(),
                MostViewedPosts = await GetMostViewedPostsAsync(10),
                MostCommentedPosts = await GetMostCommentedPostsAsync(10),
                TopAuthors = await GetTopAuthorsAsync(startDate.Value, endDate.Value, 5),
                PostsByCategory = await GetPostsByCategoryAsync(),
                PostsByTag = await GetPostsByTagAsync(),
                DailyPostStats = await GetDailyPostStatsAsync(startDate.Value, endDate.Value)
            };

            // Calculate engagement metrics
            viewModel.AverageViewsPerPost = viewModel.PublishedPosts > 0 
                ? viewModel.TotalViews / viewModel.PublishedPosts 
                : 0;
            
            viewModel.AverageCommentsPerPost = viewModel.PublishedPosts > 0 
                ? (double)viewModel.TotalComments / viewModel.PublishedPosts 
                : 0;

            return View(viewModel);
        }

        public async Task<IActionResult> SystemLogs(DateTime? date, string level = "All", int page = 1)
        {
            date ??= DateTime.UtcNow.Date;
            var pageSize = 100;

            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            var logFiles = new List<SystemLogEntry>();

            if (Directory.Exists(logsDirectory))
            {
                var files = Directory.GetFiles(logsDirectory, $"*{date.Value:yyyyMMdd}*.log")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .ToList();

                foreach (var file in files)
                {
                    var lines = await System.IO.File.ReadAllLinesAsync(file);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var logEntry = ParseLogLine(line);
                            if (logEntry != null && (level == "All" || logEntry.Level == level))
                            {
                                logFiles.Add(logEntry);
                            }
                        }
                    }
                }
            }

            var pagedLogs = logFiles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = logFiles.Count;
            ViewBag.SelectedDate = date.Value;
            ViewBag.SelectedLevel = level;

            return View(pagedLogs);
        }

        [HttpPost]
        public async Task<IActionResult> ExportUserActivityReport(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var data = await GetUserActivityDataForExportAsync(startDate, endDate);

            if (format.ToLower() == "csv")
            {
                var csv = GenerateCsvFromUserActivity(data);
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", 
                    $"UserActivity_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
            }
            else if (format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", 
                    $"UserActivity_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.json");
            }

            return BadRequest("Invalid export format");
        }

        [HttpPost]
        public async Task<IActionResult> ExportBlogStatsReport(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var data = await GetBlogStatsDataForExportAsync(startDate, endDate);

            if (format.ToLower() == "csv")
            {
                var csv = GenerateCsvFromBlogStats(data);
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", 
                    $"BlogStats_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
            }
            else if (format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", 
                    $"BlogStats_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.json");
            }

            return BadRequest("Invalid export format");
        }

        private async Task<Dictionary<string, int>> GetUsersByRoleAsync()
        {
            var result = new Dictionary<string, int>();
            var roles = await _context.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var count = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                result[role.Name ?? "Unknown"] = count.Count;
            }

            return result;
        }

        private async Task<List<DailyStatistic>> GetDailyRegistrationsAsync(DateTime startDate, DateTime endDate)
        {
            return await _userManager.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new DailyStatistic
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        private async Task<List<UserActivitySummary>> GetTopActiveUsersAsync(DateTime startDate, DateTime endDate)
        {
            var loginCounts = await _auditLogger.GetUserLoginCountsAsync(startDate, endDate, 10);
            
            var result = new List<UserActivitySummary>();
            foreach (var item in loginCounts)
            {
                var user = await _userManager.FindByIdAsync(item.UserId.ToString());
                if (user != null)
                {
                    result.Add(new UserActivitySummary
                    {
                        UserId = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        LoginCount = item.Count,
                        LastLoginDate = item.LastLogin
                    });
                }
            }

            return result;
        }

        private async Task<double> CalculateUserGrowthRateAsync(DateTime startDate, DateTime endDate)
        {
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).TotalDays);
            var previousPeriodEnd = startDate.AddDays(-1);

            var currentPeriodUsers = await _userManager.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .CountAsync();

            var previousPeriodUsers = await _userManager.Users
                .Where(u => u.CreatedAt >= previousPeriodStart && u.CreatedAt <= previousPeriodEnd)
                .CountAsync();

            if (previousPeriodUsers == 0) return currentPeriodUsers * 100;
            
            return ((double)(currentPeriodUsers - previousPeriodUsers) / previousPeriodUsers) * 100;
        }

        private async Task<List<PostSummary>> GetMostViewedPostsAsync(int count)
        {
            return await _context.Set<BlogPost>()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .Select(p => new PostSummary
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ViewCount = p.ViewCount,
                    CommentCount = p.Comments.Count,
                    PublishedAt = p.PublishedAt
                })
                .ToListAsync();
        }

        private async Task<List<PostSummary>> GetMostCommentedPostsAsync(int count)
        {
            return await _context.Set<BlogPost>()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.Comments.Count)
                .Take(count)
                .Select(p => new PostSummary
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ViewCount = p.ViewCount,
                    CommentCount = p.Comments.Count,
                    PublishedAt = p.PublishedAt
                })
                .ToListAsync();
        }

        private async Task<List<AuthorStatistic>> GetTopAuthorsAsync(DateTime startDate, DateTime endDate, int count)
        {
            return await _context.Set<BlogPost>()
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .GroupBy(p => p.AuthorId)
                .Select(g => new
                {
                    AuthorId = g.Key,
                    PostCount = g.Count(),
                    TotalViews = g.Sum(p => p.ViewCount)
                })
                .OrderByDescending(a => a.PostCount)
                .Take(count)
                .ToListAsync()
                .ContinueWith(async task =>
                {
                    var results = new List<AuthorStatistic>();
                    foreach (var item in await task)
                    {
                        var user = await _userManager.FindByIdAsync(item.AuthorId.ToString());
                        if (user != null)
                        {
                            results.Add(new AuthorStatistic
                            {
                                AuthorId = item.AuthorId,
                                AuthorName = $"{user.FirstName} {user.LastName}",
                                PostCount = item.PostCount,
                                TotalViews = item.TotalViews
                            });
                        }
                    }
                    return results;
                })
                .Result;
        }

        private async Task<Dictionary<string, int>> GetPostsByCategoryAsync()
        {
            return await _context.Set<BlogPost>()
                .Where(p => p.CategoryId != null)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        private async Task<Dictionary<string, int>> GetPostsByTagAsync()
        {
            return await _context.Set<BlogPostTag>()
                .GroupBy(pt => pt.Tag.Name)
                .Select(g => new { Tag = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Tag, x => x.Count);
        }

        private async Task<List<DailyStatistic>> GetDailyPostStatsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<BlogPost>()
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new DailyStatistic
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        private SystemLogEntry? ParseLogLine(string line)
        {
            try
            {
                // Simple log parsing - adjust based on your log format
                var parts = line.Split(new[] { ' ' }, 4);
                if (parts.Length >= 4)
                {
                    return new SystemLogEntry
                    {
                        Timestamp = DateTime.Parse($"{parts[0]} {parts[1]}"),
                        Level = parts[2].Trim('[', ']'),
                        Message = parts[3]
                    };
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }

        private async Task<object> GetUserActivityDataForExportAsync(DateTime startDate, DateTime endDate)
        {
            // Implement data gathering for export
            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                Users = await _userManager.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .Select(u => new
                    {
                        u.UserName,
                        u.Email,
                        u.CreatedAt,
                        u.IsActive
                    })
                    .ToListAsync()
            };
        }

        private async Task<object> GetBlogStatsDataForExportAsync(DateTime startDate, DateTime endDate)
        {
            // Implement data gathering for export
            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                Posts = await _context.Set<BlogPost>()
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .Select(p => new
                    {
                        p.Title,
                        p.Slug,
                        p.ViewCount,
                        CommentCount = p.Comments.Count,
                        p.CreatedAt,
                        p.PublishedAt
                    })
                    .ToListAsync()
            };
        }

        private string GenerateCsvFromUserActivity(object data)
        {
            // Implement CSV generation
            return "UserName,Email,CreatedAt,IsActive\n";
        }

        private string GenerateCsvFromBlogStats(object data)
        {
            // Implement CSV generation
            return "Title,Slug,ViewCount,CommentCount,CreatedAt,PublishedAt\n";
        }
    }
}