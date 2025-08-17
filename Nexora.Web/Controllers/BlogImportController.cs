using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Web.Models.ViewModels;
using System.Text.Json;
using ClosedXML.Excel;
using Nexora.Application.Helpers;
using System.Drawing;

namespace Nexora.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class BlogImportController : BaseController
{
    private readonly IBlogImportService _importService;
    private readonly ILogger<BlogImportController> _logger;

    public BlogImportController(
        IBlogImportService importService,
        ILogger<BlogImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new BlogImportViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(BlogImportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            // Parse Excel file
            var parseResult = await _importService.ParseExcelFileAsync(model.ExcelFile);
            
            if (!parseResult.IsSuccess || parseResult.Data == null)
            {
                SetErrorMessage(parseResult.ErrorMessage ?? "Failed to parse Excel file");
                return View("Index", model);
            }

            // Save temp file for later processing
            var tempFileResult = await _importService.SaveTempFileAsync(model.ExcelFile);
            if (!tempFileResult.IsSuccess)
            {
                SetErrorMessage(tempFileResult.ErrorMessage ?? "Failed to save file");
                return View("Index", model);
            }

            // Validate data
            var options = new BlogImportOptions
            {
                UpdateExisting = model.UpdateExisting,
                CreateCategories = model.CreateCategories,
                CreateTags = model.CreateTags,
                ImportMode = model.Mode.ToString()
            };

            var validationResult = await _importService.ValidateImportDataAsync(parseResult.Data, options);
            
            if (!validationResult.IsSuccess || validationResult.Data == null)
            {
                SetErrorMessage(validationResult.ErrorMessage ?? "Validation failed");
                _importService.DeleteTempFile(tempFileResult.Data);
                return View("Index", model);
            }

            // Create preview model
            var previewModel = new BlogImportPreviewViewModel
            {
                TempFileName = tempFileResult.Data,
                Mode = model.Mode,
                UpdateExisting = model.UpdateExisting,
                CreateCategories = model.CreateCategories,
                CreateTags = model.CreateTags
            };

            // Process rows for preview
            foreach (var row in validationResult.Data.Rows)
            {
                var importRow = new BlogImportRow
                {
                    RowNumber = row.RowNumber,
                    Title = row.Title,
                    Slug = row.Slug,
                    Content = row.Content,
                    Summary = row.Summary,
                    Category = row.Category,
                    Tags = string.Join(", ", row.Tags),
                    Author = row.Author,
                    PublishDate = row.PublishDate,
                    IsPublished = row.IsPublished,
                    IsFeatured = row.IsFeatured,
                    MetaTitle = row.MetaTitle,
                    MetaDescription = row.MetaDescription,
                    MetaKeywords = row.MetaKeywords,
                    FeaturedImage = row.FeaturedImage,
                    ViewCount = row.ViewCount,
                    IsValid = row.IsValid,
                    Errors = row.ValidationErrors
                };

                if (row.IsValid)
                {
                    previewModel.ValidRows.Add(importRow);
                }
                else
                {
                    previewModel.InvalidRows.Add(importRow);
                    previewModel.ValidationErrors[$"Row {row.RowNumber}"] = row.ValidationErrors;
                }
            }

            // Calculate statistics
            previewModel.Statistics = new ImportStatistics
            {
                TotalRows = validationResult.Data.Rows.Count,
                ValidRows = previewModel.ValidRows.Count,
                InvalidRows = previewModel.InvalidRows.Count
            };

            return View("Preview", previewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Excel file");
            SetErrorMessage("An error occurred while processing the file");
            return View("Index", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(string tempFileName, ImportMode mode, bool updateExisting, bool createCategories, bool createTags)
    {
        try
        {
            // Load temp file
            var loadResult = await _importService.LoadTempFileAsync(tempFileName);
            
            if (!loadResult.IsSuccess || loadResult.Data == null)
            {
                SetErrorMessage(loadResult.ErrorMessage ?? "Failed to load temp file");
                return RedirectToAction(nameof(Index));
            }

            // Process import
            var options = new BlogImportOptions
            {
                UpdateExisting = updateExisting,
                CreateCategories = createCategories,
                CreateTags = createTags,
                ImportMode = mode.ToString(),
                SkipInvalidRows = true
            };

            // Validate again
            var validationResult = await _importService.ValidateImportDataAsync(loadResult.Data, options);
            
            if (!validationResult.IsSuccess || validationResult.Data == null)
            {
                SetErrorMessage(validationResult.ErrorMessage ?? "Validation failed");
                return RedirectToAction(nameof(Index));
            }

            // Process the import
            var processResult = await _importService.ProcessImportAsync(validationResult.Data, options);
            
            if (!processResult.IsSuccess || processResult.Data == null)
            {
                SetErrorMessage(processResult.ErrorMessage ?? "Import processing failed");
                return RedirectToAction(nameof(Index));
            }

            // Clean up temp file
            _importService.DeleteTempFile(tempFileName);

            // Create result view model
            var resultModel = new BlogImportResult
            {
                Success = true,
                Message = "Import completed successfully",
                ImportedCount = processResult.Data.SuccessCount,
                UpdatedCount = processResult.Data.UpdatedCount,
                FailedCount = processResult.Data.FailedCount,
                Statistics = new ImportStatistics
                {
                    TotalRows = processResult.Data.SuccessCount + processResult.Data.UpdatedCount + 
                               processResult.Data.FailedCount + processResult.Data.SkippedCount,
                    ValidRows = processResult.Data.SuccessCount + processResult.Data.UpdatedCount,
                    InvalidRows = processResult.Data.FailedCount
                }
            };

            if (processResult.Data.Errors.Any())
            {
                resultModel.Errors = processResult.Data.Errors.Select(e => $"Row {e.RowNumber}: {e.Error}").ToList();
            }

            SetSuccessMessage($"Successfully imported {resultModel.ImportedCount} posts and updated {resultModel.UpdatedCount} posts");
            
            return View("Result", resultModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming import");
            SetErrorMessage("An error occurred while processing the import");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelImport(string tempFileName)
    {
        try
        {
            // Clean up temp file
            _importService.DeleteTempFile(tempFileName);
            SetInfoMessage("Import cancelled");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling import");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        try
        {
            // Create a sample Excel template
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BlogPosts");
            
            // Add headers
            var headers = new[] 
            { 
                "Title", "Slug", "Content", "Summary", "Category", "Tags", 
                "Author", "PublishDate", "IsPublished", "IsFeatured",
                "MetaTitle", "MetaDescription", "MetaKeywords", "FeaturedImage", "ViewCount"
            };
            
            // Add title row with merged cells
            var titleRange = ExcelHelper.MergeCells(worksheet, 1, 1, 1, headers.Length);
            titleRange.Value = "Blog Import Template";
            ExcelHelper.CenterAlign(titleRange);
            ExcelHelper.SetFontSize(titleRange, 16);
            ExcelHelper.SetFontStyle(titleRange, bold: true);
            ExcelHelper.SetBackgroundColor(titleRange, Color.DarkBlue);
            ExcelHelper.SetFontColor(titleRange, Color.White);
            
            // Add instruction row
            var instructionRange = ExcelHelper.MergeCells(worksheet, 2, 1, 2, headers.Length);
            instructionRange.Value = "Fill in the data below. Required fields: Title, Content. Leave Slug empty to auto-generate.";
            ExcelHelper.CenterAlign(instructionRange);
            ExcelHelper.SetBackgroundColor(instructionRange, Color.LightYellow);
            ExcelHelper.WrapText(instructionRange);
            
            // Add headers with formatting using helper
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(3, i + 1);
                cell.Value = headers[i];
            }
            
            var headerRange = worksheet.Range(3, 1, 3, headers.Length);
            ExcelHelper.ApplyHeaderStyle(headerRange);
            
            // Add multiple sample data rows (adjusted row numbers due to title and instruction rows)
            // Row 1 - Complete example
            worksheet.Cell(4, 1).Value = "Getting Started with ASP.NET Core MVC";
            worksheet.Cell(4, 2).Value = "getting-started-aspnet-core-mvc";
            worksheet.Cell(4, 3).Value = "<h2>Introduction</h2><p>ASP.NET Core MVC is a powerful framework for building web applications.</p>";
            worksheet.Cell(4, 4).Value = "Learn the basics of ASP.NET Core MVC framework";
            worksheet.Cell(4, 5).Value = "Technology";
            worksheet.Cell(4, 6).Value = "aspnet, mvc, dotnet, programming";
            worksheet.Cell(4, 7).Value = "John Developer";
            worksheet.Cell(4, 8).Value = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            worksheet.Cell(4, 9).Value = "true";
            worksheet.Cell(4, 10).Value = "false";
            worksheet.Cell(4, 11).Value = "Getting Started with ASP.NET Core MVC - Complete Guide";
            worksheet.Cell(4, 12).Value = "A comprehensive guide to getting started with ASP.NET Core MVC";
            worksheet.Cell(4, 13).Value = "ASP.NET Core, MVC, tutorial, guide";
            worksheet.Cell(4, 14).Value = "/images/blog/aspnet-core-mvc.jpg";
            worksheet.Cell(4, 15).Value = 1250;
            
            // Row 2 - Example with auto-generated slug
            worksheet.Cell(5, 1).Value = "Understanding Entity Framework Core";
            worksheet.Cell(5, 2).Value = ""; // Empty slug - will be auto-generated
            worksheet.Cell(5, 3).Value = "<p>Entity Framework Core is a modern ORM for .NET applications.</p>";
            worksheet.Cell(5, 4).Value = "Deep dive into Entity Framework Core";
            worksheet.Cell(5, 5).Value = "Technology";
            worksheet.Cell(5, 6).Value = "ef core, orm, database";
            worksheet.Cell(5, 7).Value = "Sarah Smith";
            worksheet.Cell(5, 8).Value = DateTime.Now.AddDays(-25).ToString("yyyy-MM-dd");
            worksheet.Cell(5, 9).Value = "yes"; // Different boolean format
            worksheet.Cell(5, 10).Value = "no";
            worksheet.Cell(5, 11).Value = "";
            worksheet.Cell(5, 12).Value = "";
            worksheet.Cell(5, 13).Value = "";
            worksheet.Cell(5, 14).Value = "";
            worksheet.Cell(5, 15).Value = 850;
            
            // Row 3 - Draft post example
            worksheet.Cell(6, 1).Value = "Building RESTful APIs";
            worksheet.Cell(6, 2).Value = "building-restful-apis";
            worksheet.Cell(6, 3).Value = "<p>Learn how to build RESTful APIs with best practices.</p>";
            worksheet.Cell(6, 4).Value = "API development guide";
            worksheet.Cell(6, 5).Value = "Web Development";
            worksheet.Cell(6, 6).Value = "api, rest, web services";
            worksheet.Cell(6, 7).Value = "Mike Johnson";
            worksheet.Cell(6, 8).Value = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd");
            worksheet.Cell(6, 9).Value = "false"; // Not published
            worksheet.Cell(6, 10).Value = "0";
            worksheet.Cell(6, 11).Value = "";
            worksheet.Cell(6, 12).Value = "";
            worksheet.Cell(6, 13).Value = "";
            worksheet.Cell(6, 14).Value = "";
            worksheet.Cell(6, 15).Value = 0;
            
            // Apply alternating row colors to data rows
            ExcelHelper.ApplyAlternatingRowColors(worksheet, 4, 6, 1, headers.Length);
            
            // Add borders to all data
            var dataRange = worksheet.Range(3, 1, 6, headers.Length);
            ExcelHelper.AddBorders(dataRange);
            
            // Highlight required fields
            ExcelHelper.SetBackgroundColor(worksheet.Range(3, 1, 3, 1), Color.LightCoral); // Title header
            ExcelHelper.SetBackgroundColor(worksheet.Range(3, 3, 3, 3), Color.LightCoral); // Content header
            
            // Format date column
            ExcelHelper.FormatAsDate(worksheet.Range(4, 8, 6, 8));
            
            // Add dropdown validation for boolean fields
            for (int row = 4; row <= 6; row++)
            {
                ExcelHelper.AddDropdownList(worksheet.Cell(row, 9), "true", "false", "yes", "no", "1", "0");
                ExcelHelper.AddDropdownList(worksheet.Cell(row, 10), "true", "false", "yes", "no", "1", "0");
            }
            
            // Auto-fit columns
            ExcelHelper.AutoFitColumns(worksheet);
            
            // Freeze panes to keep headers visible
            ExcelHelper.FreezePanes(worksheet, 4, 1);
            
            // Convert to byte array
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            var fileBytes = memoryStream.ToArray();
            
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                       $"BlogImportTemplate_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            SetErrorMessage("Failed to create template");
            return RedirectToAction(nameof(Index));
        }
    }
}