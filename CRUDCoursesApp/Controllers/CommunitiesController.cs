using CRUDCoursesApp.ViewModels;
using Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;

namespace CRUDCoursesApp.Controllers;

[Route("communautes")]
public class CommunitiesController(IUsersService users, IComunitiesService communities, IComunityMembersService members, IPostsService posts) : Controller
{
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        try
        {
            var community = communities.GetComunityByComId(id);
            var currentUser = users.GetAllUsers().OrderBy(u => u.Name).First();
            ViewBag.MemberCount = members.GetComunityMembersCount(id);
            ViewBag.IsMember = members.IsComunityMember(id, currentUser.UserId);
            return View(new CommunityDetailViewModel
            {
                Community = community,
                Feed = posts.GetCommunityFeed(id),
                CurrentUserName = currentUser.Name ?? "Membre"
            });
        }
        catch (ArgumentNullException) { return NotFound(); }
    }

    [HttpPost("creer")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120 || (description?.Length ?? 0) > 254)
        {
            TempData["Error"] = "Ajoutez un nom (120 caractères maximum) et une description de 254 caractères maximum.";
            return RedirectToAction("Index", "Home");
        }
        var user = users.GetAllUsers().OrderBy(u => u.Name).First();
        var created = communities.AddComunity(new AddComunityRequest { TeacherId = user.UserId, Name = name.Trim(), Description = description?.Trim() });
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpPost("{id:guid}/rejoindre")]
    [ValidateAntiForgeryToken]
    public IActionResult Join(Guid id)
    {
        if (communities.GetAllComunities().All(c => c.Id != id)) return NotFound();
        var user = users.GetAllUsers().OrderBy(u => u.Name).First();
        if (!members.IsComunityMember(id, user.UserId)) members.AddComunityMember(new AddComunityMemberRequest { ComunityId = id, UserId = user.UserId, Role = ComunityRole.Member });
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/publications")]
    [ValidateAntiForgeryToken]
    public IActionResult Post(Guid id, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 120 || string.IsNullOrWhiteSpace(body) || body.Length > 250)
        {
            TempData["Error"] = "Le titre (120 caractères) et le message (250 caractères) sont requis.";
            return RedirectToAction(nameof(Details), new { id });
        }
        var user = users.GetAllUsers().OrderBy(u => u.Name).First();
        if (!members.IsComunityMember(id, user.UserId)) return Forbid();
        posts.AddPost(new AddPostRequest { ComunityId = id, UserId = user.UserId, Title = title.Trim(), Body = body.Trim() });
        return RedirectToAction(nameof(Details), new { id });
    }
}
