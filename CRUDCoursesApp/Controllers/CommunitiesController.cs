using CRUDCoursesApp.ViewModels;
using Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;

namespace CRUDCoursesApp.Controllers 
{


    [Route("communities")]
    public class CommunitiesController : Controller
    {

        private readonly IUsersService _usersService;
        private readonly IPostsService _postsService;
        private readonly IComunitiesService _communitiesService;

        private readonly IComunityMembersService _comunityMembersService;


        public CommunitiesController(
            IUsersService usersService,
            IPostsService postsService,
            IComunitiesService communitiesService,
            IComunityMembersService communityMembersService
            )
        {
            _usersService = usersService;
            _communitiesService = communitiesService;
            _postsService = postsService;
            _comunityMembersService = communityMembersService;
        }


        [HttpGet("{id:guid}")]
        public IActionResult Details([FromRoute] Guid id)
        {
            try
            {
                ComunityResponse community = _communitiesService.GetComunityByComId(id);
                UserResponse currentUser = _usersService.GetAllUsers().OrderBy(u => u.Name).First();
                ViewBag.MemberCount = _comunityMembersService.GetComunityMembersCount(id);
                ViewBag.IsMember = _comunityMembersService.IsComunityMember(id, currentUser.UserId);
                return View(new CommunityDetailViewModel
                {
                    Community = community,
                    Feed = _postsService.GetCommunityFeed(id),
                    CurrentUserName = currentUser.Name ?? "Membre"
                });
            }
            catch (ArgumentNullException) { 
                return NotFound();
            }
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm]string name, [FromForm] string description)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 120 || (description?.Length ?? 0) > 254)
            {
                TempData["Error"] = "Ajoutez un nom (120 caractères maximum) et une description de 254 caractères maximum.";
                return RedirectToAction("Index", "Home");
            }
            UserResponse user = _usersService.GetAllUsers().OrderBy(u => u.Name).First();
            ComunityResponse created = _communitiesService.AddComunity(new AddComunityRequest { TeacherId = user.UserId, Name = name.Trim(), Description = description?.Trim() });
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        [HttpPost("{id:guid}/join")]
        [ValidateAntiForgeryToken]
        public IActionResult Join([FromRoute] Guid id)
        {
            if (_communitiesService.GetAllComunities().All(c => c.Id != id))
                return NotFound();

            UserResponse user = _usersService.GetAllUsers().OrderBy(u => u.Name).First();

            if (!_comunityMembersService.IsComunityMember(id, user.UserId))
                _comunityMembersService.AddComunityMember(new AddComunityMemberRequest { ComunityId = id, UserId = user.UserId, Role = ComunityRole.Member });
           
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("{id:guid}/posts")]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromRoute]Guid id, [FromForm] string title, [FromForm] string body)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length > 120 || string.IsNullOrWhiteSpace(body) || body.Length > 250)
            {
                TempData["Error"] = "Le titre (120 caractères) et le message (250 caractères) sont requis.";
                return RedirectToAction(nameof(Details), new { id });
            }

            UserResponse user = _usersService.GetAllUsers().OrderBy(u => u.Name).First();
            if (!_comunityMembersService.IsComunityMember(id, user.UserId)) 
                return Forbid();

            _postsService.AddPost(new AddPostRequest { ComunityId = id, UserId = user.UserId, Title = title.Trim(), Body = body.Trim() });
           
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
