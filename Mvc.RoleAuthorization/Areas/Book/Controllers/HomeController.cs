using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.RoleAuthorization.Areas.Book.Controllers;

[Area("Book")]
public class HomeController : Controller
{
    [Authorize("Authorization")]
    public IActionResult Index()
    {
        return View();
    }
}
