using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nexora.Web.Models;

public class LogViewModel
{
    public IEnumerable<dynamic> Logs { get; set; } = new List<dynamic>();
    public SelectList? TableNames { get; set; }
    public string? SelectedTableName { get; set; }
    public int? EntityId { get; set; }
    public string? SelectedAction { get; set; }
    public string Source { get; set; } = "all";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public int TotalRecords { get; set; }
}

public class EntityHistoryViewModel
{
    public string TableName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public IEnumerable<dynamic> Logs { get; set; } = new List<dynamic>();
}