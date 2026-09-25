using Microsoft.AspNetCore.Mvc;

namespace CRUDCoursesApp.Controllers
{
    public class StaticController : Controller
    {

        [HttpGet]
        [Route("/home")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        [Route("/contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        [Route("/services")]
        public IActionResult Services()
        {
            return View();
        }

        [HttpGet]
        [Route("/about-us")]
        public IActionResult About()
        {
            return View();
        }


        [HttpGet]
        [Route("/register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [Route("/authenticate")]
        public IActionResult Authenticate()
        {
            return View();
        }
    }
}
