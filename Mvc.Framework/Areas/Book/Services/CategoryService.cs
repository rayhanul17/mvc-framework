using mvc.framework.Services;
using mvc.framework.Areas.Book.Models;
using mvc.framework.Areas.Book.Data;
using mvc.framework.Areas.Book.Controllers;
using mvc.framework.Areas.Book.Models.ViewModel;
using mvc.framework.Areas.Book.Models.Entity;

namespace mvc.framework.Areas.Book.Services
{
    public class CategoryService : BaseService<Category>
    {
        public CategoryService(CategoryRepository repository) : base(repository) { }

        public async Task AddFromViewModelAsync(CategoryViewModel viewModel, string user)
        {
            var entity = new Category
            {
                Name = viewModel.Name,
                Description = viewModel.Description
            };
            await AddAsync(entity, user);
        }

        public async Task UpdateFromViewModelAsync(CategoryViewModel viewModel, string user)
        {
            var entity = await GetByIdAsync(viewModel.Id);
            if (entity != null)
            {
                entity.Name = viewModel.Name;
                entity.Description = viewModel.Description;
                await UpdateAsync(entity, user);
            }
        }
    }
}
