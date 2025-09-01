using System;

namespace MRCMS.Core.Models.Entities
{
    public class Log : BaseEntity
    {
        public required string Level { get; set; }
        public required string Message { get; set; }
        public string Exception { get; set; } = string.Empty;
        public string Properties { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
    }
    
    public class LogArchive : Log
    {
        public DateTime ArchivedAt { get; set; }
    }
}