using System;

namespace MRCMS.Core.Models.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }
        public required string Url { get; set; }
        public required string HttpMethod { get; set; }
        public required string Description { get; set; }
        
        public virtual Role Role { get; set; } = null!;
    }
}