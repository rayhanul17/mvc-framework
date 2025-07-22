using mvc.framework.Models;
using System.ComponentModel.DataAnnotations;

namespace mvc.framework.Areas.Book.Models.Entity
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
