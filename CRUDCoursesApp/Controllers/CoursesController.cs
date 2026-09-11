using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDCoursesApp.Controllers
{
    public class CoursesController : Controller
    {

        private readonly ICoursesService _coursesService;


        public CoursesController(ICoursesService coursesService)
        {
            _coursesService = coursesService;
        }


        [HttpGet]
        [Route("/courses")]
        public IActionResult Index()
        {
            try
            {
                _coursesService.SeedMockCourses();
                List<CourseResponse>? coursesResponse = _coursesService.GetAllCourses();
                return View(coursesResponse);
            }
            catch (Exception ex) {

                ViewBag.Exception = ex.Message;
                return View(null);

            }
        }


        [HttpPost]
        [Route("/courses/add")]
        public IActionResult AddCourse([FromBody] string? CourseName, [FromBody] string? CourseDescription)
        {
            try
            {
                CourseAddRequest newCourse = new CourseAddRequest() 
                {
                    CourseName = CourseName,
                    CourseDescription = CourseDescription
                };


                CourseResponse? newCourseAddResponse = _coursesService.AddCourse(newCourse);
                List<CourseResponse>? coursesResponse = _coursesService.GetAllCourses();

                return View("Index", coursesResponse);
            }
            catch (Exception ex)
            {

                ViewBag.Exception = ex.Message;
                return View("Index", null);

            }
        }


        [HttpGet]
        [Route("/courses/show/{CourseId:guid}")]
        public IActionResult ShowCourse([FromRoute] Guid CourseId)
        {
            try
            {
                CourseResponse foundCourseResponse = _coursesService.GetCourseById(CourseId);
                return View(foundCourseResponse);

            }catch(ArgumentNullException ex)
            {
                ViewBag.Exception=ex.Message;
                return LocalRedirect("/not-found");
            }
            catch (Exception ex)
            {
                ViewBag.Exception = ex.Message;
                return View(null);

            }
        }




        [HttpPost]
        [Route("/courses/update/{CourseId:guid}")]
        public IActionResult UpdateCourse([FromRoute] Guid CourseId, [FromBody] string? CourseName, [FromBody] string? CourseDescription)
        {
            return View();
        }


        [HttpGet]
        [Route("/courses/delete/{CourseId:guid}")]
        public IActionResult DeletePage([FromRoute] Guid CourseId)
        {
            return View();
        }

        [HttpPost]
        [Route("/courses/delete/{CourseId:guid}")]
        public IActionResult DeleteCourse([FromRoute] Guid CourseId)
        {
            bool isDeleted = _coursesService.DeleteCourseById(CourseId);

            if (isDeleted == false) {
                ViewBag.ErrorMessage = "Operation canceled.";
                return View($"/courses/delete/{CourseId}");
            } else
            {

                return LocalRedirect("/courses");
            }
        }



        [HttpGet]
        [Route("/not-found")]
        public IActionResult ShowNotFound()
        {

            return View();
        }

    }
}
