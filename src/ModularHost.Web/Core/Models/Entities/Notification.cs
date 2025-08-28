using System;
using System.Collections.Generic;

namespace ModularHost.Web.Core.Models.Entities
{
    public class Notification : BaseEntity
    {
        public required string Title { get; set; }
        public required string Message { get; set; }
        public bool IsPermanent { get; set; }
        public string TargetRoles { get; set; } = string.Empty;
        public string TargetUsers { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public string Type { get; set; } = "info";
        public string Icon { get; set; } = "fa-bell";
        public string Url { get; set; } = string.Empty;
        public string ReadBy { get; set; } = string.Empty;
        
        public bool IsReadByUser(Guid userId)
        {
            if (string.IsNullOrEmpty(ReadBy)) return false;
            return ReadBy.Contains($",{userId},");
        }
        
        public void MarkAsReadByUser(Guid userId)
        {
            if (string.IsNullOrEmpty(ReadBy))
                ReadBy = $",{userId},";
            else if (!IsReadByUser(userId))
                ReadBy += $"{userId},";
        }
    }
}