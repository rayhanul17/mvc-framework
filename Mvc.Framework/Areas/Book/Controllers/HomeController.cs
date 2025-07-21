using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mvc.framework.Areas.Book.Controllers;

[Area("Book")]
public class HomeController : Controller
{
    [Authorize("Authorization")]
    public IActionResult Index()
    {
        return View();
    }
}
