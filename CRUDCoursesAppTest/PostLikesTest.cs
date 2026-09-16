using Entities;
using System;
using Xunit;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    public class PostLikesTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public PostLikesTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #region BehaviorTest
        [Fact]
        public void NewPostLike_DefaultLikedAtIsRecent()
        {
            // Arrange
            DateTime before = DateTime.UtcNow;
            PostLike like = new PostLike();
            DateTime after = DateTime.UtcNow;

            _outputHelper.WriteLine($"LikedAt: {like.LikedAt:o}");

            // Assert
            Assert.True(like.LikedAt >= before.AddSeconds(-1) && like.LikedAt <= after.AddSeconds(1));
        }

        [Fact]
        public void AssociateLike_WithPost_SetsRelation()
        {
            // Arrange
            Post post = new Post();
            PostLike like = new PostLike() { UserId = Guid.NewGuid(), Post = post, PostId = post.Id };

            // Act
            post.Likes.Add(like);

            // Assert
            Assert.Contains(like, post.Likes);
            Assert.Equal(post, like.Post);
            Assert.Equal(post.Id, like.PostId);
        }

        [Fact]
        public void MultipleLikes_FromDifferentUsers_AreStored()
        {
            // Arrange
            Post post = new Post();
            var like1 = new PostLike() { UserId = Guid.NewGuid(), Post = post };
            var like2 = new PostLike() { UserId = Guid.NewGuid(), Post = post };

            // Act
            post.Likes.Add(like1);
            post.Likes.Add(like2);

            // Assert
            Assert.Equal(2, post.Likes.Count);
            Assert.Contains(like1, post.Likes);
            Assert.Contains(like2, post.Likes);
        }

        #endregion
    }
}
