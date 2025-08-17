namespace Nexora.Web.Models;

public class AuditLogViewModel
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoggedAt { get; set; }
    public string? UserName { get; set; }
    public string? UserId { get; set; }
}

public class AuditLogDataTableRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public DataTableSearch? Search { get; set; }
    public List<DataTableColumn>? Columns { get; set; }
    public List<DataTableOrder>? Order { get; set; }
}

public class DataTableSearch
{
    public string? Value { get; set; }
    public bool Regex { get; set; }
}

public class DataTableColumn
{
    public string? Data { get; set; }
    public string? Name { get; set; }
    public bool Searchable { get; set; }
    public bool Orderable { get; set; }
    public DataTableSearch? Search { get; set; }
}

public class DataTableOrder
{
    public int Column { get; set; }
    public string? Dir { get; set; }
}

public class AuditLogDataTableResponse
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<AuditLogViewModel> Data { get; set; } = new();
}