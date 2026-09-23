using CRUDCoursesApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Users;

namespace CRUDCoursesApp.Controllers 
{ 

    public class HomeController : Controller
    {

        private readonly IUsersService _usersService;
        private readonly IComunitiesService _comunitiesService;
        private readonly IPostsService _postsService;


        public HomeController(IUsersService usersService, IComunitiesService communitiesService, IPostsService postsService)
        {
            _usersService = usersService;
            _comunitiesService = communitiesService;
            _postsService = postsService;
        }

        [Route("/")]
        public IActionResult Index([FromQuery] string? q)
        {
            UserResponse currentUser = _usersService.GetAllUsers().OrderBy(u => u.Name).First();
            List<ComunityResponse> allCommunities = _comunitiesService.GetAllComunities();
            if (!string.IsNullOrWhiteSpace(q)) 
                allCommunities = allCommunities.Where(c => c.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true || c.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();
        
            return View(new CommunityHomeViewModel
            {
                CurrentUserId = currentUser.UserId,
                CurrentUserName = currentUser.Name ?? "Membre",
                Communities = allCommunities,
                Feed = _postsService.GetCommunityFeed()
            });
        }
    }
}
