using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    public interface IPostsRepository
    {
        /// <summary>
        /// Gets all the posts, newest first. The entities are not tracked.
        /// </summary>
        /// <returns>The list of all the posts.</returns>
        Task<List<Post>> GetAllPosts();

        /// <summary>
        /// Gets the number of posts in the database.
        /// </summary>
        /// <returns>The posts count.</returns>
        Task<int> GetPostCount();

        /// <summary>
        /// Gets a post by its id. The entity is not tracked.
        /// </summary>
        /// <param name="PostId">The post id.</param>
        /// <returns>The post, or null if it does not exist.</returns>
        Task<Post?> GetPostByPostId(Guid PostId);

        /// <summary>
        /// Adds a new post to the database.
        /// </summary>
        /// <param name="NewPost">The post to add.</param>
        /// <returns>The added post.</returns>
        Task<Post?> AddPost(Post NewPost);

        /// <summary>
        /// Deletes a post by its id.
        /// </summary>
        /// <param name="PostId">The post id.</param>
        /// <returns>The number of rows deleted (0 if the post does not exist).</returns>
        Task<int> DeletePostByPostId(Guid PostId);

        /// <summary>
        /// Updates the editable fields of a post (title, body, images, additionals, modification date).
        /// </summary>
        /// <param name="PostToUpdate">The post carrying the new values.</param>
        /// <returns>The updated post.</returns>
        Task<Post?> UpdatePost(Post PostToUpdate);

        /// <summary>
        /// Gets the feed, newest first, with the author and community information.
        /// </summary>
        /// <param name="CommunityId">The community to target, or null for the global feed.</param>
        /// <returns>The feed items (30 for the global feed, 100 for a community).</returns>
        Task<List<PostFeedItem>> GetCommunityFeed(Guid? CommunityId = null);

        /// <summary>
        /// Gets all the posts of a community, newest first.
        /// </summary>
        /// <param name="ComId">The community id.</param>
        /// <returns>The posts of the community.</returns>
        Task<List<Post>> GetAllPostsByComunity(Guid ComId);

        /// <summary>
        /// Gets the posts matching a predicate, newest first. The predicate is translated to SQL.
        /// </summary>
        /// <param name="Predicate">The filter to apply (it must include the community filter).</param>
        /// <returns>The matching posts.</returns>
        Task<List<Post>> GetAllFilteredPostsByComunitiy(Expression<Func<Post, bool>> Predicate);

        /// <summary>
        /// Gets the number of reports for each of the given posts.
        /// </summary>
        /// <param name="PostIds">The ids of the posts.</param>
        /// <returns>A dictionary PostId to reports count (posts without report are absent).</returns>
        Task<Dictionary<Guid, int>> GetReportCountsByPostIds(List<Guid> PostIds);

        /// <summary>
        /// Checks if a community exists.
        /// </summary>
        /// <param name="ComId">The community id.</param>
        /// <returns>True if the community exists.</returns>
        Task<bool> CommunityExists(Guid ComId);

        /// <summary>
        /// Checks if a user exists.
        /// </summary>
        /// <param name="UserId">The user id.</param>
        /// <returns>True if the user exists.</returns>
        Task<bool> UserExists(Guid UserId);
    }
}
