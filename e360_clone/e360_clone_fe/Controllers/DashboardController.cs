using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class DashboardController : Controller
    {
        // Main dashboard - auto redirect based on role
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirect to role-specific dashboard
            return role switch
            {
                "Student" => RedirectToAction("Student"),
                "Teacher" => RedirectToAction("Teacher"),
                "Parent" => RedirectToAction("Parent"),
                "Admin" or "SuperAdmin" or "Staff" => RedirectToAction("School"),
                _ => RedirectToAction("LMS")
            };
        }

        // Admin/School Dashboard - Admin, Staff ONLY
        [HttpGet]
        public IActionResult School()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || !IsAdminOrStaff(role))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // Student Dashboard - Student ONLY
        [HttpGet]
        public IActionResult Student()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Student")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // Teacher Dashboard - Teacher ONLY
        [HttpGet]
        public IActionResult Teacher()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Teacher")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // Parent Dashboard - Parent ONLY
        [HttpGet]
        public IActionResult Parent()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Parent")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // LMS Dashboard - ALL ROLES
        [HttpGet]
        public IActionResult LMS()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // Helper method
        private bool IsAdminOrStaff(string role)
        {
            return role == "Admin" || role == "SuperAdmin" || role == "Staff";
        }
    }
}
