using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;
using ClosedXML.Excel;

namespace Nexora.Application.Services;

public class BlogImportService : IBlogImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlogPostService _blogPostService;
    private readonly IBlogCategoryService _categoryService;
    private readonly ILogger<BlogImportService> _logger;
    private readonly string _tempPath;

    public BlogImportService(
        IUnitOfWork unitOfWork,
        IBlogPostService blogPostService,
        IBlogCategoryService categoryService,
        ILogger<BlogImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _blogPostService = blogPostService;
        _categoryService = categoryService;
        _logger = logger;
        _tempPath = Path.Combine(Path.GetTempPath(), "BlogImports");
        
        // Ensure temp directory exists
        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
        
    }

    public async Task<Result<BlogImportData>> ParseExcelFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Result<BlogImportData>.Failure("No file uploaded");
            }

            var importData = new BlogImportData
            {
                FileName = file.FileName
            };

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                return Result<BlogImportData>.Failure("No worksheet found in the Excel file");
            }

            // Get the used range
            var range = worksheet.RangeUsed();
            if (range == null)
            {
                return Result<BlogImportData>.Failure("Excel file is empty");
            }
            
            var rowCount = range.RowCount();
            var columnCount = range.ColumnCount();
            
            if (rowCount < 2)
            {
                return Result<BlogImportData>.Failure("Excel file must contain at least a header row and one data row");
            }

            // Read headers from first row
            var firstRow = range.Row(1);
            for (int col = 1; col <= columnCount; col++)
            {
                var header = firstRow.Cell(col).GetValue<string>()?.Trim() ?? $"Column{col}";
                importData.Headers.Add(header);
            }

            // Read data rows
            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new BlogImportRowData
                {
                    RowNumber = row
                };

                var currentRow = range.Row(row);
                // Read all cell values
                for (int col = 1; col <= columnCount; col++)
                {
                    var header = importData.Headers[col - 1];
                    var cell = currentRow.Cell(col);
                    object value = null;
                    
                    // Get value based on cell type
                    if (cell.HasFormula)
                    {
                        value = cell.CachedValue;
                    }
                    else if (cell.DataType == XLDataType.DateTime)
                    {
                        value = cell.GetDateTime();
                    }
                    else if (cell.DataType == XLDataType.Number)
                    {
                        value = cell.GetDouble();
                    }
                    else if (cell.DataType == XLDataType.Boolean)
                    {
                        value = cell.GetBoolean();
                    }
                    else
                    {
                        value = cell.GetValue<string>();
                    }
                    
                    rowData.Data[header] = value;
                }

                // Parse specific fields
                ParseRowData(rowData);
                
                importData.Rows.Add(rowData);
            }

            return Result<BlogImportData>.Success(importData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel file");
            return Result<BlogImportData>.Failure($"Error parsing Excel file: {ex.Message}");
        }
    }

    private void ParseRowData(BlogImportRowData row)
    {
        // Title
        row.Title = GetStringValue(row.Data, "Title", "title");
        
        // Slug
        row.Slug = GetStringValue(row.Data, "Slug", "slug", "URL", "url");
        if (string.IsNullOrWhiteSpace(row.Slug) && !string.IsNullOrWhiteSpace(row.Title))
        {
            row.Slug = GenerateSlug(row.Title);
        }

        // Content
        row.Content = GetStringValue(row.Data, "Content", "content", "Body", "body", "Description", "description");
        
        // Summary
        row.Summary = GetStringValue(row.Data, "Summary", "summary", "Excerpt", "excerpt");
        
        // Category
        row.Category = GetStringValue(row.Data, "Category", "category");
        
        // Tags
        var tagsString = GetStringValue(row.Data, "Tags", "tags", "Tag", "tag");
        if (!string.IsNullOrWhiteSpace(tagsString))
        {
            row.Tags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .Where(t => !string.IsNullOrWhiteSpace(t))
                                 .ToList();
        }
        
        // Author
        row.Author = GetStringValue(row.Data, "Author", "author", "AuthorName", "author_name", "CreatedBy", "created_by");
        
        // Publish Date
        row.PublishDate = GetDateTimeValue(row.Data, "PublishDate", "publish_date", "PublishedDate", "published_date", "Date", "date");
        
        // IsPublished
        row.IsPublished = GetBoolValue(row.Data, "IsPublished", "is_published", "Published", "published", "Status", "status");
        
        // IsFeatured
        row.IsFeatured = GetBoolValue(row.Data, "IsFeatured", "is_featured", "Featured", "featured");
        
        // Meta fields
        row.MetaTitle = GetStringValue(row.Data, "MetaTitle", "meta_title", "SEOTitle", "seo_title");
        row.MetaDescription = GetStringValue(row.Data, "MetaDescription", "meta_description", "SEODescription", "seo_description");
        row.MetaKeywords = GetStringValue(row.Data, "MetaKeywords", "meta_keywords", "Keywords", "keywords");
        
        // Featured Image
        row.FeaturedImage = GetStringValue(row.Data, "FeaturedImage", "featured_image", "Image", "image", "ImageUrl", "image_url");
        
        // View Count
        row.ViewCount = GetIntValue(row.Data, "ViewCount", "view_count", "Views", "views");
    }

    private string? GetStringValue(Dictionary<string, object?> data, params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (data.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString()?.Trim();
            }
            
            // Try case-insensitive match
            var matchedKey = data.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (matchedKey != null && data[matchedKey] != null)
            {
                return data[matchedKey]?.ToString()?.Trim();
            }
        }
        return null;
    }

    private DateTime? GetDateTimeValue(Dictionary<string, object?> data, params string[] possibleKeys)
    {
        var stringValue = GetStringValue(data, possibleKeys);
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        if (DateTime.TryParse(stringValue, out var date))
            return date;

        // Try parsing with different formats
        string[] formats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd HH:mm:ss" };
        if (DateTime.TryParseExact(stringValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return date;

        return null;
    }

    private bool GetBoolValue(Dictionary<string, object?> data, params string[] possibleKeys)
    {
        var stringValue = GetStringValue(data, possibleKeys)?.ToLower();
        if (string.IsNullOrWhiteSpace(stringValue))
            return false;

        return stringValue == "true" || stringValue == "yes" || stringValue == "1" || 
               stringValue == "published" || stringValue == "active";
    }

    private int GetIntValue(Dictionary<string, object?> data, params string[] possibleKeys)
    {
        var stringValue = GetStringValue(data, possibleKeys);
        if (string.IsNullOrWhiteSpace(stringValue))
            return 0;

        if (int.TryParse(stringValue, out var value))
            return value;

        return 0;
    }

    private string GenerateSlug(string title)
    {
        // Convert to lowercase
        var slug = title.ToLowerInvariant();
        
        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");
        
        // Remove non-alphanumeric characters except hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"\-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        return slug;
    }

    public async Task<Result<BlogImportData>> ValidateImportDataAsync(BlogImportData data, BlogImportOptions options)
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var existingSlugs = await context.Set<BlogPost>()
                .Select(p => p.Slug)
                .ToListAsync();
            
            var existingCategories = await context.Set<BlogCategory>()
                .Select(c => c.Name.ToLower())
                .ToListAsync();

            var seenSlugs = new HashSet<string>();

            foreach (var row in data.Rows)
            {
                row.ValidationErrors.Clear();
                row.IsValid = true;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(row.Title))
                {
                    row.ValidationErrors.Add("Title is required");
                    row.IsValid = false;
                }

                if (string.IsNullOrWhiteSpace(row.Content))
                {
                    row.ValidationErrors.Add("Content is required");
                    row.IsValid = false;
                }

                if (string.IsNullOrWhiteSpace(row.Slug))
                {
                    row.ValidationErrors.Add("Slug is required");
                    row.IsValid = false;
                }
                else
                {
                    // Check for duplicate slugs in the import data
                    if (seenSlugs.Contains(row.Slug))
                    {
                        row.ValidationErrors.Add($"Duplicate slug '{row.Slug}' in import data");
                        row.IsValid = false;
                    }
                    seenSlugs.Add(row.Slug);

                    // Check for existing slugs in database
                    if (existingSlugs.Contains(row.Slug))
                    {
                        if (options.ImportMode == "CreateNew")
                        {
                            row.ValidationErrors.Add($"Slug '{row.Slug}' already exists");
                            row.IsValid = false;
                        }
                        else if (options.ImportMode == "UpdateExisting")
                        {
                            // This is expected for updates
                        }
                    }
                }

                // Validate category
                if (!string.IsNullOrWhiteSpace(row.Category))
                {
                    if (!existingCategories.Contains(row.Category.ToLower()) && !options.CreateCategories)
                    {
                        row.ValidationErrors.Add($"Category '{row.Category}' does not exist");
                        row.IsValid = false;
                    }
                }

                // Validate date
                if (row.PublishDate.HasValue && row.PublishDate.Value > DateTime.Now.AddYears(10))
                {
                    row.ValidationErrors.Add("Publish date seems too far in the future");
                    row.IsValid = false;
                }

                // Validate URLs if present
                if (!string.IsNullOrWhiteSpace(row.FeaturedImage))
                {
                    if (!Uri.IsWellFormedUriString(row.FeaturedImage, UriKind.RelativeOrAbsolute))
                    {
                        row.ValidationErrors.Add("Featured image URL is not valid");
                        row.IsValid = false;
                    }
                }
            }

            return Result<BlogImportData>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating import data");
            return Result<BlogImportData>.Failure($"Error validating import data: {ex.Message}");
        }
    }

    public async Task<Result<BlogImportProcessResult>> ProcessImportAsync(BlogImportData data, BlogImportOptions options)
    {
        try
        {
            var result = new BlogImportProcessResult();
            var context = _unitOfWork.GetDbContext();
            
            // Get or create categories
            var categoryCache = new Dictionary<string, BlogCategory>();
            
            foreach (var row in data.Rows.Where(r => r.IsValid || !options.SkipInvalidRows))
            {
                try
                {
                    if (!row.IsValid && options.SkipInvalidRows)
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // Get or create category
                    BlogCategory? category = null;
                    if (!string.IsNullOrWhiteSpace(row.Category))
                    {
                        var categoryKey = row.Category.ToLower();
                        if (!categoryCache.TryGetValue(categoryKey, out category))
                        {
                            category = await context.Set<BlogCategory>()
                                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryKey);
                            
                            if (category == null && options.CreateCategories)
                            {
                                category = new BlogCategory
                                {
                                    Name = row.Category,
                                    Slug = GenerateSlug(row.Category),
                                    Description = $"Imported category: {row.Category}",
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow
                                };
                                context.Set<BlogCategory>().Add(category);
                                await _unitOfWork.SaveChangesAsync();
                            }
                            
                            if (category != null)
                            {
                                categoryCache[categoryKey] = category;
                            }
                        }
                    }

                    // Check if post exists
                    var existingPost = await context.Set<BlogPost>()
                        .FirstOrDefaultAsync(p => p.Slug == row.Slug);

                    if (existingPost != null)
                    {
                        if (options.ImportMode == "CreateNew")
                        {
                            result.FailedCount++;
                            result.Errors.Add(new BlogImportError
                            {
                                RowNumber = row.RowNumber,
                                Error = $"Post with slug '{row.Slug}' already exists",
                                RowData = row.Data
                            });
                            continue;
                        }
                        else if (options.ImportMode == "UpdateExisting" || options.ImportMode == "CreateOrUpdate")
                        {
                            // Update existing post
                            existingPost.Title = row.Title!;
                            existingPost.Content = row.Content!;
                            existingPost.Summary = row.Summary;
                            existingPost.CategoryId = category?.Id ?? 0;
                            existingPost.IsPublished = row.IsPublished;
                            // IsFeatured doesn't exist in BlogPost entity
                            existingPost.MetaTitle = row.MetaTitle;
                            existingPost.MetaDescription = row.MetaDescription;
                            existingPost.MetaKeywords = row.MetaKeywords;
                            existingPost.FeaturedImageUrl = row.FeaturedImage;
                            existingPost.UpdatedAt = DateTime.UtcNow;
                            
                            if (row.PublishDate.HasValue)
                            {
                                existingPost.PublishedDate = row.PublishDate.Value;
                            }

                            context.Set<BlogPost>().Update(existingPost);
                            result.UpdatedCount++;
                            result.UpdatedPostIds.Add(existingPost.Id);
                        }
                    }
                    else
                    {
                        if (options.ImportMode == "UpdateExisting")
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        // Create new post
                        var newPost = new BlogPost
                        {
                            Title = row.Title!,
                            Slug = row.Slug!,
                            Content = row.Content!,
                            Summary = row.Summary,
                            CategoryId = category?.Id ?? 0,
                            IsPublished = row.IsPublished,
                            // IsFeatured doesn't exist in BlogPost entity
                            MetaTitle = row.MetaTitle,
                            MetaDescription = row.MetaDescription,
                            MetaKeywords = row.MetaKeywords,
                            FeaturedImageUrl = row.FeaturedImage,
                            ViewCount = row.ViewCount,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            PublishedDate = row.PublishDate ?? DateTime.UtcNow,
                            AuthorId = row.Author // You might need to map this to actual user ID
                        };

                        context.Set<BlogPost>().Add(newPost);
                        await _unitOfWork.SaveChangesAsync();
                        
                        result.SuccessCount++;
                        result.ImportedPostIds.Add(newPost.Id);
                        
                        // Handle tags (if you have a tag system)
                        // This would require additional implementation based on your tag structure
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing row {row.RowNumber}");
                    result.FailedCount++;
                    result.Errors.Add(new BlogImportError
                    {
                        RowNumber = row.RowNumber,
                        Error = ex.Message,
                        RowData = row.Data
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            
            return Result<BlogImportProcessResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing import");
            return Result<BlogImportProcessResult>.Failure($"Error processing import: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveTempFileAsync(IFormFile file)
    {
        try
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_tempPath, fileName);
            
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            
            return Result<string>.Success(fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving temp file");
            return Result<string>.Failure($"Error saving temp file: {ex.Message}");
        }
    }

    public async Task<Result<BlogImportData>> LoadTempFileAsync(string tempFileName)
    {
        try
        {
            var filePath = Path.Combine(_tempPath, tempFileName);
            
            if (!File.Exists(filePath))
            {
                return Result<BlogImportData>.Failure("Temp file not found");
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var formFile = new FormFile(stream, 0, stream.Length, "file", tempFileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return await ParseExcelFileAsync(formFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading temp file");
            return Result<BlogImportData>.Failure($"Error loading temp file: {ex.Message}");
        }
    }

    public void DeleteTempFile(string tempFileName)
    {
        try
        {
            var filePath = Path.Combine(_tempPath, tempFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting temp file");
        }
    }
}