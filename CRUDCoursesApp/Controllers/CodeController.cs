using Microsoft.AspNetCore.Mvc;

namespace CRUDCoursesApp.Controllers
{
    public class CodeController : Controller
    {
        [Route("/not-found")]
        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
