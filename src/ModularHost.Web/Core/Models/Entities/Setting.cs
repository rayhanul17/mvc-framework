using System;

namespace MRCMS.Core.Models.Entities
{
    public class Setting : BaseEntity
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsPublic { get; set; }
    }
}