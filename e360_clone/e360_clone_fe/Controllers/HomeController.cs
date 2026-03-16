using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to appropriate dashboard based on user role
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            return role switch
            {
                "Student" => RedirectToAction("Student", "Dashboard"),
                "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                "Parent" => RedirectToAction("Parent", "Dashboard"),
                _ => RedirectToAction("School", "Dashboard")
            };
        }
    }
}
