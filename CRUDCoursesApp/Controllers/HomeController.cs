using Microsoft.AspNetCore.Mvc;

namespace CRUDCoursesApp.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
