using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Web.Models;

namespace Nexora.Web.Controllers;

[Authorize]
public class AuditLogController : Controller
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetAuditLogs([FromForm] AuditLogDataTableRequest request)
    {
        try
        {
            _logger.LogInformation($"GetAuditLogs called - Draw: {request.Draw}, Start: {request.Start}, Length: {request.Length}");
            
            var searchValue = request.Search?.Value;
            var sortColumn = request.Columns != null && request.Order != null && request.Order.Any()
                ? request.Columns[request.Order[0].Column].Data
                : "LoggedAt";
            var sortDirection = request.Order != null && request.Order.Any()
                ? request.Order[0].Dir
                : "desc";
            
            _logger.LogInformation($"Search: {searchValue}, Sort: {sortColumn} {sortDirection}");

            var dataResult = await _auditLogService.GetAuditLogsForDataTableAsync(
                request.Start,
                request.Length,
                searchValue,
                sortColumn,
                sortDirection,
                request.TableName,
                request.Action,
                request.UserName,
                request.StartDate,
                request.EndDate
            );

            if (!dataResult.IsSuccess || dataResult.Data == null)
            {
                _logger.LogWarning($"Failed to get audit logs: {dataResult.ErrorMessage}");
                return Json(new 
                {
                    draw = request.Draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = dataResult.ErrorMessage ?? "Failed to load audit logs"
                });
            }

            var data = new List<AuditLogViewModel>();
            foreach (System.Data.DataRow row in dataResult.Data.Rows)
            {
                try
                {
                    data.Add(new AuditLogViewModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        TableName = row["TableName"]?.ToString() ?? string.Empty,
                        EntityId = row["EntityId"] != DBNull.Value ? Convert.ToInt32(row["EntityId"]) : 0,
                        Action = row["Action"]?.ToString() ?? string.Empty,
                        Changes = row["Changes"] != DBNull.Value ? row["Changes"].ToString() : null,
                        IpAddress = row["IpAddress"] != DBNull.Value ? row["IpAddress"].ToString() : null,
                        UserAgent = row["UserAgent"] != DBNull.Value ? row["UserAgent"].ToString() : null,
                        LoggedAt = row["LoggedAt"] != DBNull.Value ? Convert.ToDateTime(row["LoggedAt"]) : DateTime.Now,
                        UserId = row["UserId"] != DBNull.Value ? row["UserId"].ToString() : null,
                        UserName = row["UserName"] != DBNull.Value ? row["UserName"].ToString() : 
                                  (row["FullName"] != DBNull.Value ? row["FullName"].ToString() : "System")
                    });
                }
                catch (Exception rowEx)
                {
                    _logger.LogError(rowEx, "Error processing row in audit log data");
                    // Continue processing other rows
                }
            }

            var totalCountResult = await _auditLogService.GetTotalCountAsync();
            var filteredCountResult = await _auditLogService.GetFilteredCountAsync(
                searchValue, 
                request.TableName, 
                request.Action, 
                request.UserName, 
                request.StartDate, 
                request.EndDate);

            var response = new 
            {
                draw = request.Draw,
                recordsTotal = totalCountResult.IsSuccess ? totalCountResult.Data : 0,
                recordsFiltered = filteredCountResult.IsSuccess ? filteredCountResult.Data : 0,
                data = data.Select(d => new
                {
                    id = d.Id,
                    tableName = d.TableName,
                    entityId = d.EntityId,
                    action = d.Action,
                    changes = d.Changes,
                    ipAddress = d.IpAddress,
                    userAgent = d.UserAgent,
                    loggedAt = d.LoggedAt,
                    userId = d.UserId,
                    userName = d.UserName
                })
            };
            
            _logger.LogInformation($"Returning {data.Count} audit log records");
            
            return Json(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return Json(new 
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<object>(),
                error = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _auditLogService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return NotFound();
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetTableNames()
    {
        try
        {
            var result = await _auditLogService.GetUniqueTableNamesAsync();
            if (result.IsSuccess && result.Data != null)
            {
                return Json(result.Data);
            }
            return Json(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table names");
            return Json(new List<string>());
        }
    }
}