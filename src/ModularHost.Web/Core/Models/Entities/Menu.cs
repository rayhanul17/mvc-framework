using System;
using System.Collections.Generic;

namespace MRCMS.Core.Models.Entities
{
    public class Menu : BaseEntity
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public Guid? ParentId { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public string Icon { get; set; } = string.Empty;
        public string? ClaimType { get; set; }
        public string? ModuleName { get; set; }
        
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
    }
}