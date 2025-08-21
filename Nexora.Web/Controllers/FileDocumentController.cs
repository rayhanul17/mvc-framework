using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using System.Data;
using System.Security.Claims;

namespace Nexora.Web.Controllers;

[Authorize]
public class FileDocumentController : BaseController
{
    private readonly IFileDocumentService _fileDocumentService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileDocumentController> _logger;

    public FileDocumentController(IFileDocumentService fileDocumentService, IWebHostEnvironment environment, ILogger<FileDocumentController> logger)
    {
        _fileDocumentService = fileDocumentService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Load categories and extensions for filters
        var categoriesResult = await _fileDocumentService.GetCategoriesAsync();
        var extensionsResult = await _fileDocumentService.GetFileExtensionsAsync();
        var totalSizeResult = await _fileDocumentService.GetTotalFileSizeAsync();
        var totalDownloadsResult = await _fileDocumentService.GetTotalDownloadCountAsync();

        ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Data : new List<string>();
        ViewBag.Extensions = extensionsResult.IsSuccess ? extensionsResult.Data : new List<string>();
        ViewBag.TotalSize = totalSizeResult.IsSuccess ? FormatFileSize(totalSizeResult.Data) : "0 B";
        ViewBag.TotalDownloads = totalDownloadsResult.IsSuccess ? totalDownloadsResult.Data : 0;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetFileDocuments()
    {
        try
        {
            // DataTables parameters
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            // Custom filters
            var category = Request.Form["category"].FirstOrDefault();
            var fileExtension = Request.Form["fileExtension"].FirstOrDefault();
            var startDateStr = Request.Form["startDate"].FirstOrDefault();
            var endDateStr = Request.Form["endDate"].FirstOrDefault();

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(startDateStr) && DateTime.TryParse(startDateStr, out var parsedStartDate))
                startDate = parsedStartDate;

            if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out var parsedEndDate))
                endDate = parsedEndDate;

            var result = await _fileDocumentService.GetFileDocumentsForDataTableAsync(
                int.Parse(draw ?? "0"),
                int.Parse(start ?? "0"),
                int.Parse(length ?? "10"),
                searchValue ?? string.Empty,
                int.Parse(sortColumn ?? "0"),
                sortColumnDirection ?? "desc",
                category,
                fileExtension,
                startDate,
                endDate
            );

            if (!result.IsSuccess)
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = result.ErrorMessage
                });
            }

            var dataTable = result.Data;
            var data = new List<object>();

            foreach (DataRow row in dataTable.Rows)
            {
                data.Add(new
                {
                    id = row["Id"],
                    originalFileName = row["OriginalFileName"],
                    category = row["Category"],
                    fileExtension = row["FileExtension"],
                    fileSize = FormatFileSize(Convert.ToInt64(row["FileSize"])),
                    fileSizeBytes = row["FileSize"],
                    downloadCount = row["DownloadCount"],
                    uploadedBy = row["UploadedBy"],
                    createdAt = row["CreatedAt"],
                    description = row["Description"],
                    tags = row["Tags"],
                    isPublic = Convert.ToBoolean(row["IsPublic"]),
                    lastDownloadedAt = row["LastDownloadedAt"],
                    filePath = row["FilePath"]
                });
            }

            return Json(new
            {
                draw = dataTable.ExtendedProperties["draw"],
                recordsTotal = dataTable.ExtendedProperties["recordsTotal"],
                recordsFiltered = dataTable.ExtendedProperties["recordsFiltered"],
                data = data
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                draw = Request.Form["draw"].FirstOrDefault(),
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<object>(),
                error = ex.Message
            });
        }
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, string category, string? description, string? tags)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload";
            return View();
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            TempData["ErrorMessage"] = "Please select a category";
            return View();
        }

        var uploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        var result = await _fileDocumentService.UploadFileAsync(file, category, description, tags, uploadedBy);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "File uploaded successfully";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.ErrorMessage;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            // Check if it's an image
            if (!file.ContentType.StartsWith("image/"))
            {
                return Json(new { success = false, message = "File must be an image" });
            }

            // Check file size (max 5MB for images)
            if (file.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Image size must be less than 5MB" });
            }

            var uploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Anonymous";
            var result = await _fileDocumentService.UploadFileAsync(file, "BlogImages", "Blog post image", null, uploadedBy);

            if (result.IsSuccess && result.Data != null)
            {
                var imageUrl = result.Data.FilePath;
                return Json(new { success = true, url = imageUrl });
            }

            return Json(new { success = false, message = result.ErrorMessage ?? "Upload failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return Json(new { success = false, message = "An error occurred while uploading the image" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var fileResult = await _fileDocumentService.GetByIdAsync(id);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                TempData["ErrorMessage"] = "File not found";
                return RedirectToAction(nameof(Index));
            }

            var file = fileResult.Data;
            var fullPath = Path.Combine(_environment.WebRootPath, file.FilePath.TrimStart('/').Replace('/', '\\'));

            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "File not found on server";
                return RedirectToAction(nameof(Index));
            }

            // Increment download count
            await _fileDocumentService.IncrementDownloadCountAsync(id);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, file.ContentType, file.OriginalFileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _fileDocumentService.DeleteFileWithPhysicalRemovalAsync(id);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "File deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}