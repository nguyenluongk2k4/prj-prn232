using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class StudentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
