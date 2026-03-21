using Microsoft.AspNetCore.Mvc;
using e360_clone.BusinessObjects;
using e360_clone_fe.Extensions;

namespace e360_clone_fe.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Check if user is logged in
            var user = HttpContext.Session.GetUser();
            
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirect to appropriate dashboard based on user role
            return user.Role switch
            {
                "Student" => RedirectToAction("Student", "Dashboard"),
                "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                "Parent" => RedirectToAction("Parent", "Dashboard"),
                "Admin" => RedirectToAction("Lms", "Dashboard"),
                _ => RedirectToAction("School", "Dashboard")
            };
        }
    }
}
