using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;

namespace CRUDCoursesApp.ViewModels;

public class CommunityDetailViewModel
{
    public ComunityResponse Community { get; set; } = null!;
    public List<FeedPostResponse> Feed { get; set; } = new();
    public string CurrentUserName { get; set; } = "";
}
