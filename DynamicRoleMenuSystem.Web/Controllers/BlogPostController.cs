using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Web.Models.ViewModels;

namespace DynamicRoleMenuSystem.Web.Controllers;

[Authorize]
public class BlogPostController : BaseController
{
    private readonly IBlogPostService _postService;
    private readonly IBlogCategoryService _categoryService;

    public BlogPostController(IBlogPostService postService, IBlogCategoryService categoryService)
    {
        _postService = postService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _postService.GetAllAsync();
        if (result.IsSuccess)
        {
            return View(result.Data);
        }

        SetErrorMessage(result.ErrorMessage ?? "Failed to load posts");
        return View(new List<BlogPost>());
    }

    public async Task<IActionResult> Create()
    {
        await LoadCategories();
        return View(new BlogPostViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategories();
            return View(model);
        }

        var post = new BlogPost
        {
            Title = model.Title,
            Summary = model.Summary,
            Content = model.Content,
            CategoryId = model.CategoryId,
            FeaturedImageUrl = model.FeaturedImageUrl,
            Tags = model.Tags,
            MetaTitle = model.MetaTitle,
            MetaDescription = model.MetaDescription,
            MetaKeywords = model.MetaKeywords,
            IsPublished = model.IsPublished,
            AuthorId = GetCurrentUserId()
        };

        // Generate slug
        if (string.IsNullOrEmpty(model.Slug))
        {
            var slugResult = await _postService.GenerateSlugAsync(model.Title);
            if (slugResult.IsSuccess)
            {
                post.Slug = slugResult.Data;
            }
        }
        else
        {
            post.Slug = model.Slug;
        }

        // Check if slug exists
        var slugExistsResult = await _postService.SlugExistsAsync(post.Slug);
        if (slugExistsResult.IsSuccess && slugExistsResult.Data)
        {
            ModelState.AddModelError("Slug", "This slug already exists");
            await LoadCategories();
            return View(model);
        }

        if (model.IsPublished)
        {
            post.PublishedDate = DateTime.UtcNow;
        }

        var result = await _postService.CreateAsync(post);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Post created successfully");
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result);
        await LoadCategories();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _postService.GetPostWithCategoryAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            SetErrorMessage("Post not found");
            return RedirectToAction(nameof(Index));
        }

        var model = new BlogPostViewModel
        {
            Id = result.Data.Id,
            Title = result.Data.Title,
            Slug = result.Data.Slug,
            Summary = result.Data.Summary,
            Content = result.Data.Content,
            CategoryId = result.Data.CategoryId,
            FeaturedImageUrl = result.Data.FeaturedImageUrl,
            Tags = result.Data.Tags,
            MetaTitle = result.Data.MetaTitle,
            MetaDescription = result.Data.MetaDescription,
            MetaKeywords = result.Data.MetaKeywords,
            IsPublished = result.Data.IsPublished
        };

        await LoadCategories();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPostViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories();
            return View(model);
        }

        var postResult = await _postService.GetByIdAsync(id);
        if (!postResult.IsSuccess || postResult.Data == null)
        {
            SetErrorMessage("Post not found");
            return RedirectToAction(nameof(Index));
        }

        var post = postResult.Data;
        post.Title = model.Title;
        post.Summary = model.Summary;
        post.Content = model.Content;
        post.CategoryId = model.CategoryId;
        post.FeaturedImageUrl = model.FeaturedImageUrl;
        post.Tags = model.Tags;
        post.MetaTitle = model.MetaTitle;
        post.MetaDescription = model.MetaDescription;
        post.MetaKeywords = model.MetaKeywords;
        post.UpdatedBy = GetCurrentUserId();

        // Handle slug
        if (!string.IsNullOrEmpty(model.Slug) && model.Slug != post.Slug)
        {
            var slugExistsResult = await _postService.SlugExistsAsync(model.Slug, id);
            if (slugExistsResult.IsSuccess && slugExistsResult.Data)
            {
                ModelState.AddModelError("Slug", "This slug already exists");
                await LoadCategories();
                return View(model);
            }
            post.Slug = model.Slug;
        }

        // Handle publishing
        if (model.IsPublished && !post.IsPublished)
        {
            post.IsPublished = true;
            post.PublishedDate = DateTime.UtcNow;
        }
        else if (!model.IsPublished && post.IsPublished)
        {
            post.IsPublished = false;
            post.PublishedDate = null;
        }

        var result = await _postService.UpdateAsync(post);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Post updated successfully");
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result);
        await LoadCategories();
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await _postService.GetPostWithCategoryAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            SetErrorMessage("Post not found");
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _postService.DeleteAsync(id);
        
        if (result.IsSuccess)
        {
            SetSuccessMessage("Post deleted successfully");
        }
        else
        {
            SetErrorMessage(result.ErrorMessage ?? "Failed to delete post");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        var result = await _postService.PublishPostAsync(id);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "Post published successfully" });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpPost]
    public async Task<IActionResult> Unpublish(int id)
    {
        var result = await _postService.UnpublishPostAsync(id);
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, message = "Post unpublished successfully" });
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    private async Task LoadCategories()
    {
        var categoriesResult = await _categoryService.GetAllActiveAsync();
        if (categoriesResult.IsSuccess && categoriesResult.Data != null)
        {
            ViewBag.Categories = new SelectList(categoriesResult.Data, "Id", "Name");
        }
        else
        {
            ViewBag.Categories = new SelectList(new List<BlogCategory>(), "Id", "Name");
        }
    }
}