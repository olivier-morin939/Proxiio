using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.Linq.Expressions;

namespace Repository
{

    public class PostsRepository : IPostsRepository
    {

        private const int DefaultFeedLimit = 30;
        private const int CommunityFeedLimit = 100;

        private readonly ApplicationDbContext _db;
        private readonly ILogger<PostsRepository>? _logger;

        public PostsRepository(ApplicationDbContext Db, ILogger<PostsRepository>? logger = null)
        {
            _db = Db;
            _logger = logger;
        }

        private void LogOperation(string method, string operation, string arguments)
        {
            _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
            _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
        }

        // Pass-through methods return the Task directly (no async/await) to avoid a state machine allocation.
        public Task<List<Post>> GetAllPosts()
        {
            LogOperation("GET", nameof(GetAllPosts), "(aucun)");
            return _db.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public Task<int> GetPostCount()
        {
            LogOperation("GET", nameof(GetPostCount), "(aucun)");
            return _db.Posts.CountAsync();
        }

        public Task<Post?> GetPostByPostId(Guid PostId)
        {
            LogOperation("GET", nameof(GetPostByPostId), $"PostId:{PostId}");
            return _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == PostId);
        }

        public async Task<Post?> AddPost(Post NewPost)
        {
            LogOperation("POST", nameof(AddPost), $"PostId:{NewPost.Id}");
            _db.Posts.Add(NewPost);
            int RowsAffected = await _db.SaveChangesAsync();

            if (RowsAffected <= 0)
                return null;

            return NewPost;
        }

        // ExecuteDeleteAsync (EF Core 7+) deletes directly in SQL without loading the entity.
        public Task<int> DeletePostByPostId(Guid PostId)
        {
            LogOperation("POST", nameof(DeletePostByPostId), $"PostId:{PostId}");
            return _db.Posts.Where(p => p.Id == PostId).ExecuteDeleteAsync();
        }

        public async Task<Post?> UpdatePost(Post PostToUpdate)
        {
            LogOperation("POST", nameof(UpdatePost), $"PostId:{PostToUpdate.Id}");
            // Attach then flag only the editable fields: no SELECT and the UPDATE only touches these columns.
            EntityEntry<Post> Entry = _db.Posts.Attach(PostToUpdate);
            Entry.Property(p => p.Title).IsModified = true;
            Entry.Property(p => p.Body).IsModified = true;
            Entry.Property(p => p.ImagesPath).IsModified = true;
            Entry.Property(p => p.AdditionalsPath).IsModified = true;
            Entry.Property(p => p.ModifiedAt).IsModified = true;

            int RowsAffected = await _db.SaveChangesAsync();

            if (RowsAffected <= 0)
                return null;

            return PostToUpdate;
        }

        public Task<List<PostFeedItem>> GetCommunityFeed(Guid? CommunityId = null)
        {
            LogOperation("GET", nameof(GetCommunityFeed), $"CommunityId:{CommunityId}");
            // Filtering on the posts before the joins reduces the rows to join
            IQueryable<Post> PostsQuery = _db.Posts.AsNoTracking();

            if (CommunityId.HasValue)
                PostsQuery = PostsQuery.Where(p => p.CommunityId == CommunityId.Value);

            IQueryable<PostFeedItem> FeedQuery =
                from p in PostsQuery
                join u in _db.Users.AsNoTracking() on p.UserId equals u.UserId
                join c in _db.Comunities.AsNoTracking() on p.CommunityId equals c.Id
                orderby p.CreatedAt descending
                select new PostFeedItem
                {
                    Post = p,
                    AuthorName = u.Name,
                    CommunityId = c.Id,
                    CommunityName = c.Name,
                    CommunityDescription = c.Description
                };

            int Limit = CommunityId.HasValue ? CommunityFeedLimit : DefaultFeedLimit;

            return FeedQuery.Take(Limit).ToListAsync();
        }

        public Task<List<Post>> GetAllPostsByComunity(Guid ComId)
        {
            LogOperation("GET", nameof(GetAllPostsByComunity), $"CommunityId:{ComId}");
            return _db.Posts.AsNoTracking().Where(p => p.CommunityId == ComId).OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public Task<List<Post>> GetAllFilteredPostsByComunitiy(Expression<Func<Post, bool>> Predicate)
        {
            LogOperation("GET", nameof(GetAllFilteredPostsByComunitiy), "predicate: reçu");
            return _db.Posts.AsNoTracking().Where(Predicate).OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Dictionary<Guid, int>> GetReportCountsByPostIds(List<Guid> PostIds)
        {
            LogOperation("GET", nameof(GetReportCountsByPostIds), $"PostIdsCount:{PostIds.Count}");
            if (PostIds.Count == 0)
                return new Dictionary<Guid, int>();

            // The grouping is done in SQL: only (PostId, Count) pairs come back instead of every report.
            return await _db.Reports.AsNoTracking()
                .Where(r => PostIds.Contains(r.PostId))
                .GroupBy(r => r.PostId)
                .Select(group => new { PostId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.PostId, item => item.Count);
        }

        public Task<bool> CommunityExists(Guid ComId)
        {
            LogOperation("GET", nameof(CommunityExists), $"CommunityId:{ComId}");
            return _db.Comunities.AnyAsync(c => c.Id == ComId);
        }

        public Task<bool> UserExists(Guid UserId)
        {
            LogOperation("GET", nameof(UserExists), $"UserId:{UserId}");
            return _db.Users.AnyAsync(u => u.UserId == UserId);
        }
    }
}
