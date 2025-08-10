namespace Nexora.Core.Common;

public class AuditLogSettings
{
    public bool Enabled { get; set; } = true;
    public ArchiveSettings ArchiveSettings { get; set; } = new();
    public int RetentionDays { get; set; } = 90;
}

public class ArchiveSettings
{
    public int Duration { get; set; } = 30;
    public string Unit { get; set; } = "day"; // min, hour, day, week, month, year
    
    public TimeSpan GetTimeSpan()
    {
        return Unit.ToLower() switch
        {
            "min" or "minute" => TimeSpan.FromMinutes(Duration),
            "hour" => TimeSpan.FromHours(Duration),
            "day" => TimeSpan.FromDays(Duration),
            "week" => TimeSpan.FromDays(Duration * 7),
            "month" => TimeSpan.FromDays(Duration * 30),
            "year" => TimeSpan.FromDays(Duration * 365),
            _ => TimeSpan.FromDays(Duration)
        };
    }
}