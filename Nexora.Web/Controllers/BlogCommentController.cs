using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Web.Controllers;

namespace Nexora.Web.Controllers;

[Authorize]
public class BlogCommentController : BaseController
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Blog Comments";
        return View();
    }

    public IActionResult Approve(int id)
    {
        SetSuccessMessage("Comment approved successfully.");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Reject(int id)
    {
        SetSuccessMessage("Comment rejected successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        SetSuccessMessage("Comment deleted successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reply(int id, string replyText)
    {
        if (string.IsNullOrWhiteSpace(replyText))
        {
            SetErrorMessage("Reply text is required.");
            return RedirectToAction(nameof(Index));
        }

        SetSuccessMessage("Reply posted successfully.");
        return RedirectToAction(nameof(Index));
    }
}