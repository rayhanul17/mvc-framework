using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MRCMS.Controllers
{
    public class ProjectController : Controller
    {

        public IActionResult Overview()
        {
            ViewData["Title"] = "Project Overview";
            return View();
        }

        public IActionResult Documentation()
        {
            ViewData["Title"] = "Documentation";
            return View();
        }

        public IActionResult API()
        {
            ViewData["Title"] = "API Documentation";
            return View();
        }
    }
}