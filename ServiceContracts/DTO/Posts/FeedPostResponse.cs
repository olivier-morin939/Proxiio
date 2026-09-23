namespace ServiceContracts.DTO.Posts;

public class FeedPostResponse
{
    public PostResponse Post { get; set; } = null!;
    public string AuthorName { get; set; } = "";
    public string CommunityName { get; set; } = "";
    public string? CommunityDescription { get; set; }
    public Guid CommunityId { get; set; }
}
