using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index(string area = "Admin")
        {
            // Redirect to appropriate dashboard based on area/role
            return RedirectToAction(area);
        }

        // Admin/School Dashboard
        [HttpGet]
        public IActionResult School()
        {
            return View();
        }

        // Student Dashboard
        [HttpGet]
        public IActionResult Student()
        {
            return View();
        }

        // Teacher Dashboard
        [HttpGet]
        public IActionResult Teacher()
        {
            return View();
        }

        // Parent Dashboard
        [HttpGet]
        public IActionResult Parent()
        {
            return View();
        }

        // LMS Dashboard
        [HttpGet]
        public IActionResult Lms()
        {
            return View();
        }
    }
}
