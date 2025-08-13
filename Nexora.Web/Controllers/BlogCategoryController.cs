using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Models.ViewModels;

namespace Nexora.Web.Controllers;

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

    // Combined Create/Edit GET action
    public async Task<IActionResult> CreateEdit(int id = 0)
    {
        BlogCategoryViewModel model;
        
        if (id > 0)
        {
            // Edit mode
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                SetErrorMessage("Category not found");
                return RedirectToAction(nameof(Index));
            }

            model = new BlogCategoryViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Slug = result.Data.Slug,
                Description = result.Data.Description,
                DisplayOrder = result.Data.DisplayOrder,
                IsActive = result.Data.IsActive
            };
        }
        else
        {
            // Create mode
            model = new BlogCategoryViewModel();
        }

        return View("CreateEdit", model);
    }

    // Combined Create/Edit POST action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(BlogCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("CreateEdit", model);
        }

        bool isCreate = model.Id == 0;
        
        if (isCreate)
        {
            // Create logic
            var category = new BlogCategory
            {
                Name = model.Name,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedBy = GetCurrentUserId()
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
                return View("CreateEdit", model);
            }

            var result = await _categoryService.CreateAsync(category);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Category created successfully", showAfterRedirect: true);
                return RedirectToAction(nameof(Index));
            }

            AddErrorsToModelState(result);
        }
        else
        {
            // Update logic
            var categoryResult = await _categoryService.GetByIdAsync(model.Id);
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
                var slugExistsResult = await _categoryService.SlugExistsAsync(model.Slug, model.Id);
                if (slugExistsResult.IsSuccess && slugExistsResult.Data)
                {
                    ModelState.AddModelError("Slug", "This slug already exists");
                    return View("CreateEdit", model);
                }
                category.Slug = model.Slug;
            }

            var result = await _categoryService.UpdateAsync(category);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Category updated successfully", showAfterRedirect: true);
                return RedirectToAction(nameof(Index));
            }

            AddErrorsToModelState(result);
        }

        return View("CreateEdit", model);
    }
    
    // Redirect old Create action to CreateEdit
    public IActionResult Create()
    {
        return RedirectToAction(nameof(CreateEdit));
    }
    
    // Redirect old Edit action to CreateEdit
    public IActionResult Edit(int id)
    {
        return RedirectToAction(nameof(CreateEdit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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