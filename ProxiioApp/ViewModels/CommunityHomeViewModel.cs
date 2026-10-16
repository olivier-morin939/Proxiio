using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;

namespace CRUDCoursesApp.ViewModels;

public class CommunityHomeViewModel
{
    public Guid CurrentUserId { get; set; }
    public string CurrentUserName { get; set; } = "";
    public List<ComunityResponse> Communities { get; set; } = new();
    public List<FeedPostResponse> Feed { get; set; } = new();
}
