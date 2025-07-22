using System.ComponentModel.DataAnnotations;

namespace mvc.framework.Areas.Book.Models.ViewModel
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

}