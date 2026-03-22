using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Get role from Session (set during login)
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirect to appropriate dashboard based on role
            // Admin/Staff → LMS Dashboard (not School)
            return role switch
            {
                "Student" => RedirectToAction("Student", "Dashboard"),
                "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                "Parent" => RedirectToAction("Parent", "Dashboard"),
                "Admin" or "SuperAdmin" or "Staff" => RedirectToAction("LMS", "Dashboard"),
                _ => RedirectToAction("LMS", "Dashboard")
            };
        }
    }
}
