using Entities.Enums;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using System;
using System.Collections.Generic;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the PostsService.
    /// </summary>
    public interface IPostsService
    {

       

        /// <summary>
        /// Get all the posts contained in the system
        /// </summary>
        /// <returns>Returns a list of DTO object of type PostResponse</returns>
        Task<List<PostResponse>> GetAllPosts();

        Task<List<FeedPostResponse>> GetCommunityFeed(Guid? communityId = null);

        /// <summary>
        /// Get all the posts contained in the desired community filtered by searchBy and searchString
        /// </summary>
        /// <param name="ComId">The desired community id to target</param>
        /// <param name="searchBy">The property name to search by</param>
        /// <param name="searchString">The content string to filter on</param>
        /// <returns>Returns a list of DTO object of type PostResponse filtered by the property name and search string of a specific community</returns>
        public Task<List<PostResponse>> GetAllFilteredPostsByComunitiy(Guid ComId, string searchBy, string searchString);

        /// <summary>
        /// Get all the posts contained in the desired community sorted by sortBy and sortOrder
        /// </summary>
        /// <param name="ComId">The desired community id to target</param>
        /// <param name="allPostsResponseFromComunity">The actual posts contained inside the community before sorting</param>
        /// <param name="sortBy">The property name to sort by</param>
        /// <param name="sortOrder">The sort order, ASC or DESC</param>
        /// <returns>Returns a list of DTO object of type PostResponse sorted by the property name and sort order of a specific community</returns>
        public Task<List<PostResponse>> GetAllSortedPostsByComunity(Guid ComId, List<PostResponse> allPostsResponseFromComunity, string sortBy, SortOption sortOrder);

        /// <summary>
        /// Add a new post to a community (or default community)
        /// </summary>
        /// <param name="addPostRequest">The DTO containing the post data</param>
        /// <returns>Returns the created PostResponse</returns>
        Task<PostResponse> AddPost(AddPostRequest? addPostRequest);

        /// <summary>
        /// Get a post by its id
        /// </summary>
        /// <param name="PostId">The post id</param>
        /// <returns>Returns a PostResponse for the specified post</returns>
        Task<PostResponse> GetPostByPostId(Guid PostId);

        /// <summary>
        /// Update an existing post
        /// </summary>
        /// <param name="updatePostRequest">DTO containing updated post fields</param>
        /// <returns>Returns the updated PostResponse</returns>
        Task<PostResponse> UpdatePost(UpdatePostRequest updatePostRequest);

        /// <summary>
        /// Delete a post by its id
        /// </summary>
        /// <param name="PostId">The post id to delete</param>
        /// <returns>True if deleted, otherwise false</returns>
        Task<bool> DeletePostByPostId(Guid PostId);

        /// <summary>
        /// Get the counts of all posts contained inside the PostsService
        /// </summary>
        /// <returns>An integer representing the number of posts in the system</returns>
        public Task<int> GetAllPostsCount();


    }
}
