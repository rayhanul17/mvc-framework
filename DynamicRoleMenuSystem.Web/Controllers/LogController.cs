using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Web.Models;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class LogController : Controller
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
        string source = "all",
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;
        
        // Get total count first for pagination (without skip/take)
        var totalLogsResult = await _logService.GetCombinedLogsAsync(
            tableName, entityId, null, startDate, endDate, action, source, null, null);
        var totalRecords = totalLogsResult.IsSuccess ? totalLogsResult.Data?.Count() ?? 0 : 0;
        
        // Get paginated logs
        var logsResult = await _logService.GetCombinedLogsAsync(
            tableName, entityId, null, startDate, endDate, action, source, skip, pageSize);
            
        if (!logsResult.IsSuccess)
        {
            TempData["Error"] = logsResult.ErrorMessage;
            return View(new LogViewModel { Logs = new List<dynamic>() });
        }
        
        var tableNamesResult = await _logService.GetTableNamesAsync();
        var tableNames = tableNamesResult.IsSuccess ? tableNamesResult.Data : new List<string>();
        
        var viewModel = new LogViewModel
        {
            Logs = logsResult.Data?.Cast<dynamic>() ?? new List<dynamic>(),
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
        
        return View(viewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> EntityHistory(string tableName, int entityId)
    {
        var logsResult = await _logService.GetCombinedLogsAsync(
            tableName, entityId, null, null, null, null, "all", null, null);
            
        if (!logsResult.IsSuccess)
        {
            TempData["Error"] = logsResult.ErrorMessage;
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
            TempData["Success"] = "Logs archived successfully";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage;
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
            TempData["Success"] = "Archived logs cleaned up successfully";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage;
        }
        
        return RedirectToAction(nameof(Index));
    }
}