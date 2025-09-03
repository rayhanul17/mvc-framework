using Microsoft.AspNetCore.Mvc;
using MRCMS.Core.Controllers;

namespace MRCMS.Controllers
{
    public class UserGuideController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PermissionManagement()
        {
            return View();
        }

        public IActionResult SystemSetup()
        {
            return View();
        }

        public IActionResult UserManagement()
        {
            return View();
        }

        public IActionResult RoleManagement()
        {
            return View();
        }

        public IActionResult MenuConfiguration()
        {
            return View();
        }

        public IActionResult Troubleshooting()
        {
            return View();
        }

        public IActionResult ApiDocumentation()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}