using Entities;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    public class PostsServiceTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public PostsServiceTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #region BehaviorTest
        [Fact]
        public void NewPost_DefaultCollectionsInitialized()
        {
            // Arrange
            Post post = new Post();

            // Act & Assert
            Assert.NotNull(post.ImagesPath);
            Assert.NotNull(post.AdditionalsPath);
            Assert.NotNull(post.Comments);
            Assert.NotNull(post.Likes);
            Assert.Empty(post.ImagesPath);
            Assert.Empty(post.AdditionalsPath);
            Assert.Empty(post.Comments);
            Assert.Empty(post.Likes);
        }

        [Fact]
        public void NewPost_CreatedAtIsSetToUtcNow()
        {
            // Arrange
            DateTime before = DateTime.UtcNow;
            Post post = new Post();
            DateTime after = DateTime.UtcNow;

            // Act
            _outputHelper.WriteLine($"CreatedAt: {post.CreatedAt:o}");

            // Assert CreatedAt is between before and after (allowing a tiny margin)
            Assert.True(post.CreatedAt >= before.AddSeconds(-1) && post.CreatedAt <= after.AddSeconds(1));
        }

        [Fact]
        public void AddLike_ToPost_IncrementsLikesCount()
        {
            // Arrange
            Post post = new Post();
            var like = new PostLike() { UserId = Guid.NewGuid(), Post = post, PostId = post.Id };

            // Act
            post.Likes.Add(like);

            // Assert
            Assert.Single(post.Likes);
            Assert.Contains(like, post.Likes);
            Assert.Equal(post, like.Post);
        }

        [Fact]
        public void AddMultipleImages_ToPost_PersistsPaths()
        {
            // Arrange
            Post post = new Post();
            var img1 = "/images/a.png";
            var img2 = "/images/b.png";

            // Act
            post.ImagesPath.Add(img1);
            post.ImagesPath.Add(img2);

            // Assert
            Assert.Equal(2, post.ImagesPath.Count);
            Assert.Contains(img1, post.ImagesPath);
            Assert.Contains(img2, post.ImagesPath);
        }
        #endregion
    }
}
