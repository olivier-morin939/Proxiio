using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Posts;

namespace Services;

public class PostsService : IPostsService
{
    private readonly UsersDbContext _db;
    public PostsService(UsersDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public PostsService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public List<PostResponse> GetAllPosts() => AddReportCounts(_db.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToList());
    public int GetAllPostsCount() => _db.Posts.Count();

    public List<FeedPostResponse> GetCommunityFeed(Guid? communityId = null)
    {
        // The query to execute to get the displayed feed when no community id is given
        var query = from post in _db.Posts.AsNoTracking()
                    join user in _db.Users.AsNoTracking() on post.UserId equals user.UserId
                    join community in _db.Comunities.AsNoTracking() on post.CommunityId equals community.Id
                    select new { post, user, community };


        // The query to execute when the community id is given
        if (communityId.HasValue) 
            query = query.Where(item => item.community.Id == communityId.Value);

        // Return the results from the sql requests
        return query.OrderByDescending(item => item.post.CreatedAt).Take(communityId.HasValue ? 100 : 30).ToList()
            .Select(item => new FeedPostResponse
            {
                Post = item.post.ToPostResponse(), AuthorName = item.user.Name ?? "Membre",
                CommunityName = item.community.Name ?? "Communauté", CommunityDescription = item.community.Description,
                CommunityId = item.community.Id
            }).ToList();
    }

    public PostResponse AddPost(AddPostRequest? request)
    {
        // Check if the DTO request is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO request
        Helpers.HelpersValidation.ModelValidation(request);

        // Validating the mandatory fields
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) 
            throw new ArgumentException("A title and message are required.");

        // Check if the community exists
        if (!_db.Comunities.Any(c => c.Id == request.ComunityId))
            throw new ArgumentException("Community not found.", nameof(request.ComunityId));

        // Check if the user exists
        if (!_db.Users.Any(u => u.UserId == request.UserId))
            throw new ArgumentException("User not found.", nameof(request.UserId));

        // Convert the DTO request into an entity post class
        Post post = request.ToPost();

        // Add it to the db
        _db.Posts.Add(post);
        _db.SaveChanges();

        // Return the DTO response
        return post.ToPostResponse();
    }

    public PostResponse GetPostByPostId(Guid PostId)
    {
        // Search the corresponding post and return the DTO response
        Post post = _db.Posts.AsNoTracking().FirstOrDefault(p => p.Id == PostId) ?? throw new KeyNotFoundException($"Post {PostId} was not found.");
        return post.ToPostResponse();
    }

    public PostResponse UpdatePost(UpdatePostRequest request)
    {

        // Check if the DTO request is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation of the DTO request
        Helpers.HelpersValidation.ModelValidation(request);

        // Searching the corresponding post
        Post post = _db.Posts.FirstOrDefault(p => p.Id == request.Id) ?? throw new KeyNotFoundException($"Post {request.Id} was not found.");
        
        // Checking for the mandatory fields
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("A title and message are required.");

        // Fields updation
        post.Title = request.Title;
        post.Body = request.Body;
        post.ImagesPath = request.ImagesPath ?? new();
        post.AdditionalsPath = request.AdditionalsPath ?? new();
        post.ModifiedAt = DateTime.UtcNow;
        _db.SaveChanges();

        // Return the DTO response
        return post.ToPostResponse();
    }

    public bool DeletePostByPostId(Guid PostId)
    {
        // Searching the corresponding post
        Post? post = _db.Posts.FirstOrDefault(p => p.Id == PostId);

        // The post is not found
        if (post is null)
            return false;

        // Remove the post from the db
        _db.Posts.Remove(post);
        _db.SaveChanges();
        return true;
    }

    public List<PostResponse> GetAllPostsByComunity(Guid ComId)
    {
        // Checking if the community id is valid
        if (ComId == Guid.Empty) 
            throw new ArgumentException("Community id cannot be empty.", nameof(ComId));

        // Give the numbers of reports per posts of the given community id
        return AddReportCounts(_db.Posts.AsNoTracking().Where(p => p.CommunityId == ComId).OrderByDescending(p => p.CreatedAt).ToList());
    }

    public List<PostResponse> GetAllFilteredPostsByComunitiy(Guid ComId, string searchBy, string searchString)
    {
        // Get all posts
        List<PostResponse> all = GetAllPostsByComunity(ComId);

        // Checking if the searchBy and searchString is null or empty
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) 
            return all;

        // Filtering the informations based on the given searchBy and searchString
        return searchBy switch
        {
            nameof(PostResponse.UserId) => all.Where(p => p.UserId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            nameof(PostResponse.Title) => all.Where(p => p.Title?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList(),
            nameof(PostResponse.Body) => all.Where(p => p.Body?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList(),
            nameof(PostResponse.CreatedAt) => all.Where(p => p.CreatedAt.ToString("dd MMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => all
        };
    }

    public List<PostResponse> GetAllSortedPostsByComunity(Guid ComId, List<PostResponse> posts, string sortBy, SortOption sortOrder)
    {

        // Checking if the sortBy is null or empty
        if (string.IsNullOrEmpty(sortBy)) 
            return posts;

        // Checking if the order is descending
        bool desc = sortOrder != SortOption.ASC;


        // Sort based on the sortBy fields and sort order
        return sortBy switch
        {
            nameof(PostResponse.UserId) => desc ? posts.OrderByDescending(p => p.UserId).ToList() : posts.OrderBy(p => p.UserId).ToList(),
            nameof(PostResponse.Title) => desc ? posts.OrderByDescending(p => p.Title).ToList() : posts.OrderBy(p => p.Title).ToList(),
            nameof(PostResponse.Body) => desc ? posts.OrderByDescending(p => p.Body).ToList() : posts.OrderBy(p => p.Body).ToList(),
            nameof(PostResponse.CreatedAt) => desc ? posts.OrderByDescending(p => p.CreatedAt).ToList() : posts.OrderBy(p => p.CreatedAt).ToList(),
            _ => posts
        };
    }

    private List<PostResponse> AddReportCounts(List<Post> posts)
    {
        // Checking if there is posts
        if (posts.Count == 0) 
            return new();

        // Get all the ids of the posts
        var ids = posts.Select(p => p.Id).ToList();

        // Get the number of reports of every posts
        var counts = _db.Reports.AsNoTracking().Where(r => ids.Contains(r.PostId))
            .GroupBy(r => r.PostId).Select(group => new { PostId = group.Key, Count = group.Count() })
            .ToDictionary(item => item.PostId, item => item.Count);

        // Set the found count and return the DTO response 
        return posts.Select(post =>
        {
            PostResponse response = post.ToPostResponse();
            response.ReportsCount = counts.GetValueOrDefault(post.Id);
            return response;
        }).ToList();
    }
}
