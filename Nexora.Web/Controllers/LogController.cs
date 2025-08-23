using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Application.Interfaces;
using Nexora.Web.Attributes;
using Nexora.Web.Models;

namespace Nexora.Web.Controllers;

[DynamicPermissionAuthorize]
public class LogController : BaseController
{
    private readonly ILogService _logService;
    private readonly ILogger<LogController> _logger;
    
    public LogController(ILogService logService, ILogger<LogController> logger)
    {
        _logService = logService;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        // Get table names for filter dropdown
        var tableNamesResult = await _logService.GetTableNamesAsync();
        var tableNames = tableNamesResult.IsSuccess ? tableNamesResult.Data : new List<string>();
        
        ViewBag.TableNames = new SelectList(tableNames);
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportLogs(
        string? source, 
        string? tableName, 
        int? rowId,
        int? entityId, 
        string? action,
        string? username,
        string? ipAddress,
        DateTime? startDate,
        DateTime? endDate,
        string? keyword)
    {
        try
        {
            // Get filtered logs for export
            var logsResult = await _logService.GetCombinedLogsAsync(
                tableName,
                entityId,
                username,
                startDate,
                endDate,
                action,
                source ?? "log",
                null, // No pagination for export
                null,
                ipAddress,
                keyword
            );
            
            if (!logsResult.IsSuccess || logsResult.Data == null)
            {
                TempData["ErrorMessage"] = "Failed to export logs";
                return RedirectToAction(nameof(Index));
            }
            
            // Convert to CSV format
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Date/Time,Table,Entity ID,Action,User,IP Address,Changes,Old Values,New Values,Status");
            
            foreach (dynamic log in logsResult.Data)
            {
                csv.AppendLine($"{log.Id},{log.LoggedAt},{log.TableName},{log.EntityId},{log.Action}," +
                              $"{log.User?.UserName ?? "System"},{log.IpAddress ?? "-"}," +
                              $"\"{log.Changes ?? ""}\",\"{log.OldValues ?? ""}\",\"{log.NewValues ?? ""}\"," +
                              $"{(log.IsArchived ? "Archived" : "Active")}");
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"logs_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting logs");
            TempData["ErrorMessage"] = "An error occurred while exporting logs";
            return RedirectToAction(nameof(Index));
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> GetLogs([FromForm] LogDataTableRequest request)
    {
        try
        {
            // Parse search value if needed
            var searchValue = request.Search?.Value;
            
            // Get sort column and direction
            var sortColumn = request.Columns != null && request.Order != null && request.Order.Any()
                ? request.Columns[request.Order[0].Column].Data
                : "LoggedAt";
            var sortDirection = request.Order != null && request.Order.Any()
                ? request.Order[0].Dir
                : "desc";
            
            // Handle row ID filter separately if provided
            if (request.RowId.HasValue)
            {
                // Get specific log by ID from appropriate source
                var logResult = await _logService.GetLogByIdAsync(request.RowId.Value, request.Source ?? "log");
                if (logResult.IsSuccess && logResult.Data != null)
                {
                    var log = logResult.Data;
                    var singleData = new List<LogDataViewModel>
                    {
                        new LogDataViewModel
                        {
                            Id = log.Id,
                            TableName = log.TableName,
                            EntityId = log.EntityId,
                            Action = log.Action,
                            OldValues = log.OldValues,
                            NewValues = log.NewValues,
                            Changes = log.Changes,
                            IpAddress = log.IpAddress,
                            UserAgent = log.UserAgent,
                            LoggedAt = log.LoggedAt,
                            UserId = log.UserId,
                            UserName = log.User?.UserName,
                            FullName = log.User?.FullName,
                            IsArchived = request.Source == "archive"
                        }
                    };
                    
                    return Json(new LogDataTableResponse
                    {
                        Draw = request.Draw,
                        RecordsTotal = 1,
                        RecordsFiltered = 1,
                        Data = singleData
                    });
                }
            }
            
            // Get the data using raw queries with all filters
            var logsResult = await _logService.GetCombinedLogsAsync(
                request.TableName,
                request.EntityId,
                request.Username, // Now filtering by username
                request.StartDate,
                request.EndDate,
                request.Action,
                request.Source ?? "log",
                request.Start,
                request.Length,
                request.IpAddress,
                request.Keyword
            );
            
            if (!logsResult.IsSuccess || logsResult.Data == null)
            {
                return Json(new LogDataTableResponse
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<LogDataViewModel>()
                });
            }
            
            // Convert to view models
            var data = new List<LogDataViewModel>();
            foreach (dynamic log in logsResult.Data)
            {
                data.Add(new LogDataViewModel
                {
                    Id = log.Id,
                    TableName = log.TableName,
                    EntityId = log.EntityId,
                    Action = log.Action,
                    OldValues = log.OldValues,
                    NewValues = log.NewValues,
                    Changes = log.Changes,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    LoggedAt = log.LoggedAt,
                    UserId = log.UserId,
                    UserName = log.User?.UserName,
                    FullName = log.User?.FullName,
                    IsArchived = log.IsArchived,
                    ArchivedAt = log.ArchivedAt
                });
            }
            
            // Get total count (without filters)
            var totalCountResult = await _logService.GetCombinedLogsAsync(
                null, null, null, null, null, null, request.Source ?? "log", null, null, null, null
            );
            var totalCount = totalCountResult.IsSuccess && totalCountResult.Data != null 
                ? totalCountResult.Data.Count() 
                : 0;
            
            // Get filtered count
            var filteredCountResult = await _logService.GetCombinedLogsAsync(
                request.TableName,
                request.EntityId,
                null,
                request.StartDate,
                request.EndDate,
                request.Action,
                request.Source ?? "log",
                null,
                null
            );
            var filteredCount = filteredCountResult.IsSuccess && filteredCountResult.Data != null 
                ? filteredCountResult.Data.Count() 
                : 0;
            
            var response = new LogDataTableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = filteredCount,
                Data = data
            };
            
            return Json(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs for DataTables");
            return Json(new LogDataTableResponse
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = new List<LogDataViewModel>()
            });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> EntityHistory(string tableName, int entityId)
    {
        var logsResult = await _logService.GetCombinedLogsAsync(
            tableName, entityId, null, null, null, null, "all", null, null);
            
        if (!logsResult.IsSuccess)
        {
            SetErrorMessage(logsResult.ErrorMessage ?? "Failed to load entity history");
            return View(new EntityHistoryViewModel { Logs = new List<dynamic>() });
        }
        
        var viewModel = new EntityHistoryViewModel
        {
            TableName = tableName,
            EntityId = entityId,
            Logs = logsResult.Data?.Cast<dynamic>() ?? new List<dynamic>()
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    [PermissionAuthorize(area: "", controller: "Log", action: "Archive")]
    public async Task<IActionResult> ArchiveLogs()
    {
        var result = await _logService.ArchiveOldLogsAsync();
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Logs archived successfully", showAfterRedirect: true);
        }
        else
        {
            SetErrorMessage(result.ErrorMessage ?? "Failed to archive logs", showAfterRedirect: true);
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [PermissionAuthorize(area: "", controller: "Log", action: "Cleanup")]
    public async Task<IActionResult> CleanupArchives()
    {
        var result = await _logService.CleanupArchivedLogsAsync();
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Archived logs cleaned up successfully", showAfterRedirect: true);
        }
        else
        {
            SetErrorMessage(result.ErrorMessage ?? "Failed to cleanup archived logs", showAfterRedirect: true);
        }
        
        return RedirectToAction(nameof(Index));
    }
}