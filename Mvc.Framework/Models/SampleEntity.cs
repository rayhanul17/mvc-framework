using System;
using System.ComponentModel.DataAnnotations;

namespace mvc.framework.Models
{
    public class SampleEntity : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public int Priority { get; set; }
    }
}