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
        public async Task<IActionResult> Index([FromQuery] string? q)
        {
            List<UserResponse> allUsers = await _usersService.GetAllUsers();
            UserResponse? currentUser = allUsers.OrderBy(u => u.Name).FirstOrDefault();
            List<ComunityResponse> allCommunities = await _comunitiesService.GetAllComunities();
            if (!string.IsNullOrWhiteSpace(q)) 
                allCommunities = allCommunities.Where(c => c.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true || c.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();
        
            return View(new CommunityHomeViewModel
            {
                CurrentUserId = currentUser?.UserId ?? Guid.Empty,
                CurrentUserName = currentUser?.Name ?? "Membre",
                Communities = allCommunities,
                Feed = await _postsService.GetCommunityFeed()
            });
        }
    }
}
