using mvc.framework.Data;
using mvc.framework.Areas.Book.Models.Entity;

namespace mvc.framework.Areas.Book.Data
{
    public class CategoryRepository : BaseRepository<Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
    }
}
