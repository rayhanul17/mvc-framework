using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Web.Controllers;

namespace Nexora.Web.Controllers;

[Authorize]
public class BlogTagController : BaseController
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Blog Tags";
        return View();
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Tag";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            SetErrorMessage("Tag name is required.");
            return View();
        }

        SetSuccessMessage($"Tag '{tagName}' created successfully.");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewData["Title"] = "Edit Tag";
        ViewData["TagId"] = id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            SetErrorMessage("Tag name is required.");
            return View();
        }

        SetSuccessMessage($"Tag updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        SetSuccessMessage("Tag deleted successfully.");
        return RedirectToAction(nameof(Index));
    }
}