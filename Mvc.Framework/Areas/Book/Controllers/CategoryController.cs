using Microsoft.AspNetCore.Mvc;
using mvc.framework.Areas.Book.Models.ViewModel;
using mvc.framework.Areas.Book.Services;

namespace mvc.framework.Areas.Book.Controllers
{
    [Area("Book")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _service;
        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var items = _service.GetAll().ToList();
            return View(items);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View(new CategoryViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await _service.AddFromViewModelAsync(viewModel, User.Identity?.Name ?? "system");
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return View(new CategoryViewModel());
            var viewModel = new CategoryViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateFromViewModelAsync(viewModel, User.Identity?.Name ?? "system");
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
