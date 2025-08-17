namespace Nexora.Web.Models;

public class LogDataTableRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public DataTableSearch? Search { get; set; }
    public List<DataTableColumn>? Columns { get; set; }
    public List<DataTableOrder>? Order { get; set; }
    
    // Additional filters for logs
    public string? Source { get; set; } = "log"; // log, archive, or all - Default to main log table
    public string? TableName { get; set; }
    public int? RowId { get; set; } // Filter by specific log row ID
    public int? EntityId { get; set; }
    public string? Action { get; set; }
    public string? Username { get; set; } // Filter by username
    public string? IpAddress { get; set; } // Filter by IP address
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Keyword { get; set; } // Search in changes, old values, new values
}

public class LogDataTableResponse
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<LogDataViewModel> Data { get; set; } = new();
}

public class LogDataViewModel
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoggedAt { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
}