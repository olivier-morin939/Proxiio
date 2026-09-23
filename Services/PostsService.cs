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
    public PostsService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public List<PostResponse> GetAllPosts() => AddReportCounts(_db.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToList());
    public int GetAllPostsCount() => _db.Posts.Count();

    public List<FeedPostResponse> GetCommunityFeed(Guid? communityId = null)
    {
        var query = from post in _db.Posts.AsNoTracking()
                    join user in _db.Users.AsNoTracking() on post.UserId equals user.UserId
                    join community in _db.Comunities.AsNoTracking() on post.CommunityId equals community.Id
                    select new { post, user, community };
        if (communityId.HasValue) query = query.Where(item => item.community.Id == communityId.Value);
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
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) throw new ArgumentException("A title and message are required.");
        if (!_db.Comunities.Any(c => c.Id == request.ComunityId)) throw new ArgumentException("Community not found.", nameof(request.ComunityId));
        if (!_db.Users.Any(u => u.UserId == request.UserId)) throw new ArgumentException("User not found.", nameof(request.UserId));
        var post = request.ToPost();
        _db.Posts.Add(post);
        _db.SaveChanges();
        return post.ToPostResponse();
    }

    public PostResponse GetPostByPostId(Guid PostId)
    {
        var post = _db.Posts.AsNoTracking().FirstOrDefault(p => p.Id == PostId) ?? throw new KeyNotFoundException($"Post {PostId} was not found.");
        return post.ToPostResponse();
    }

    public PostResponse UpdatePost(UpdatePostRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var post = _db.Posts.FirstOrDefault(p => p.Id == request.Id) ?? throw new KeyNotFoundException($"Post {request.Id} was not found.");
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) throw new ArgumentException("A title and message are required.");
        post.Title = request.Title;
        post.Body = request.Body;
        post.ImagesPath = request.ImagesPath ?? new();
        post.AdditionalsPath = request.AdditionalsPath ?? new();
        post.ModifiedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return post.ToPostResponse();
    }

    public bool DeletePostByPostId(Guid PostId)
    {
        var post = _db.Posts.FirstOrDefault(p => p.Id == PostId);
        if (post is null) return false;
        _db.Posts.Remove(post);
        _db.SaveChanges();
        return true;
    }

    public List<PostResponse> GetAllPostsByComunity(Guid ComId)
    {
        if (ComId == Guid.Empty) throw new ArgumentException("Community id cannot be empty.", nameof(ComId));
        return AddReportCounts(_db.Posts.AsNoTracking().Where(p => p.CommunityId == ComId).OrderByDescending(p => p.CreatedAt).ToList());
    }

    public List<PostResponse> GetAllFilteredPostsByComunitiy(Guid ComId, string searchBy, string searchString)
    {
        var all = GetAllPostsByComunity(ComId);
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) return all;
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
        if (string.IsNullOrEmpty(sortBy)) return posts;
        var desc = sortOrder != SortOption.ASC;
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
        if (posts.Count == 0) return new();
        var ids = posts.Select(p => p.Id).ToList();
        var counts = _db.Reports.AsNoTracking().Where(r => ids.Contains(r.PostId))
            .GroupBy(r => r.PostId).Select(group => new { PostId = group.Key, Count = group.Count() })
            .ToDictionary(item => item.PostId, item => item.Count);
        return posts.Select(post =>
        {
            var response = post.ToPostResponse();
            response.ReportsCount = counts.GetValueOrDefault(post.Id);
            return response;
        }).ToList();
    }
}
