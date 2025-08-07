using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Models.ViewModels;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class BlogCategoryController : BaseController
{
    private readonly IBlogCategoryService _categoryService;

    public BlogCategoryController(IBlogCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        if (result.IsSuccess)
        {
            return View(result.Data);
        }

        SetErrorMessage(result.ErrorMessage ?? "Failed to load categories");
        return View(new List<BlogCategory>());
    }

    public IActionResult Create()
    {
        return View(new BlogCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = new BlogCategory
        {
            Name = model.Name,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };

        // Generate slug
        if (string.IsNullOrEmpty(model.Slug))
        {
            var slugResult = await _categoryService.GenerateSlugAsync(model.Name);
            if (slugResult.IsSuccess)
            {
                category.Slug = slugResult.Data;
            }
        }
        else
        {
            category.Slug = model.Slug;
        }

        // Check if slug exists
        var slugExistsResult = await _categoryService.SlugExistsAsync(category.Slug);
        if (slugExistsResult.IsSuccess && slugExistsResult.Data)
        {
            ModelState.AddModelError("Slug", "This slug already exists");
            return View(model);
        }

        category.CreatedBy = GetCurrentUserId();
        var result = await _categoryService.CreateAsync(category);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Category created successfully");
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            SetErrorMessage("Category not found");
            return RedirectToAction(nameof(Index));
        }

        var model = new BlogCategoryViewModel
        {
            Id = result.Data.Id,
            Name = result.Data.Name,
            Slug = result.Data.Slug,
            Description = result.Data.Description,
            DisplayOrder = result.Data.DisplayOrder,
            IsActive = result.Data.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogCategoryViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var categoryResult = await _categoryService.GetByIdAsync(id);
        if (!categoryResult.IsSuccess || categoryResult.Data == null)
        {
            SetErrorMessage("Category not found");
            return RedirectToAction(nameof(Index));
        }

        var category = categoryResult.Data;
        category.Name = model.Name;
        category.Description = model.Description;
        category.DisplayOrder = model.DisplayOrder;
        category.IsActive = model.IsActive;
        category.UpdatedBy = GetCurrentUserId();

        // Handle slug
        if (!string.IsNullOrEmpty(model.Slug) && model.Slug != category.Slug)
        {
            var slugExistsResult = await _categoryService.SlugExistsAsync(model.Slug, id);
            if (slugExistsResult.IsSuccess && slugExistsResult.Data)
            {
                ModelState.AddModelError("Slug", "This slug already exists");
                return View(model);
            }
            category.Slug = model.Slug;
        }

        var result = await _categoryService.UpdateAsync(category);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Category updated successfully");
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result);
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            SetErrorMessage("Category not found");
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Category deleted successfully");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage ?? "Failed to delete category");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CheckSlugExists(string slug, int? id)
    {
        var result = await _categoryService.SlugExistsAsync(slug, id);
        return Json(!result.Data);
    }
}