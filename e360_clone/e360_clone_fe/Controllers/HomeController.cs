using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
