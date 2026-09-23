using CRUDCoursesApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDCoursesApp.Controllers;

public class HomeController(IUsersService users, IComunitiesService communities, IPostsService posts) : Controller
{
    [Route("/")]
    public IActionResult Index(string? q)
    {
        var currentUser = users.GetAllUsers().OrderBy(u => u.Name).First();
        var allCommunities = communities.GetAllComunities();
        if (!string.IsNullOrWhiteSpace(q)) allCommunities = allCommunities.Where(c => c.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true || c.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();
        return View(new CommunityHomeViewModel
        {
            CurrentUserId = currentUser.UserId,
            CurrentUserName = currentUser.Name ?? "Membre",
            Communities = allCommunities,
            Feed = posts.GetCommunityFeed()
        });
    }
}
