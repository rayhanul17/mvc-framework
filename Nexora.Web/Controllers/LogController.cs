using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nexora.Application.Interfaces;
using Nexora.Web.Models;

namespace Nexora.Web.Controllers;

[Authorize]
public class LogController : BaseController
{
    private readonly ILogService _logService;
    
    public LogController(ILogService logService)
    {
        _logService = logService;
    }
    
    public async Task<IActionResult> Index(
        string? tableName = null,
        int? entityId = null,
        string? action = null,
        string? source = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 25)
    {
        try
        {
            // Set default source to "log" (Main Table) if not specified
            if (string.IsNullOrEmpty(source))
                source = "log";
            
            // Ensure page size has a default
            if (pageSize <= 0) pageSize = 25;
            if (page <= 0) page = 1;
            
            var skip = (page - 1) * pageSize;
            
            // Get all logs without pagination first to get the count
            var allLogsResult = await _logService.GetCombinedLogsAsync(
                tableName, entityId, null, startDate, endDate, action, source, null, null);
            
            if (!allLogsResult.IsSuccess)
            {
                SetErrorMessage($"Failed to load logs: {allLogsResult.ErrorMessage}");
                return View(new LogViewModel 
                { 
                    Logs = new List<dynamic>(),
                    TableNames = new SelectList(new List<string>()),
                    CurrentPage = page,
                    PageSize = pageSize,
                    Source = source
                });
            }
            
            var allLogs = allLogsResult.Data?.ToList() ?? new List<object>();
            var totalRecords = allLogs.Count;
            
            // Debug: Log the count
            if (totalRecords == 0)
            {
                // Try to get logs directly without filtering to debug
                var debugResult = await _logService.GetCombinedLogsAsync(
                    null, null, null, null, null, null, "log", null, null);
                var debugCount = debugResult.Data?.Count() ?? 0;
                
                if (debugCount == 0)
                {
                    // No logs in database, show info message
                    SetWarningMessage($"No logs found in the database. Source: {source}");
                }
                else
                {
                    SetWarningMessage($"Found {debugCount} logs in database but filters returned 0 results.");
                }
            }
            
            // Now get the paginated subset
            var paginatedLogs = allLogs.Skip(skip).Take(pageSize).ToList();
            
            // Get table names for filter dropdown
            var tableNamesResult = await _logService.GetTableNamesAsync();
            var tableNames = tableNamesResult.IsSuccess ? tableNamesResult.Data : new List<string>();
            
            var viewModel = new LogViewModel
            {
                Logs = paginatedLogs.Cast<dynamic>(),
                TableNames = new SelectList(tableNames),
                SelectedTableName = tableName,
                EntityId = entityId,
                SelectedAction = action,
                Source = source,
                StartDate = startDate,
                EndDate = endDate,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
            
            // Debug: Add a message if we have logs but they're not showing
            if (totalRecords > 0 && !paginatedLogs.Any())
            {
                SetWarningMessage($"Found {totalRecords} logs but page {page} is out of range. Showing page 1.");
                return RedirectToAction(nameof(Index), new { 
                    tableName, entityId, action, source, startDate, endDate, 
                    page = 1, pageSize 
                });
            }
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred while loading logs: {ex.Message}");
            return View(new LogViewModel 
            { 
                Logs = new List<dynamic>(),
                TableNames = new SelectList(new List<string>()),
                CurrentPage = 1,
                PageSize = pageSize,
                Source = source
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
    [Authorize]
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
    [Authorize]
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