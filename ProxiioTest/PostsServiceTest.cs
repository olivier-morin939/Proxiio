using AutoFixture;
using Entities;
using Entities.Enums;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Posts;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace ProxiioTest
{
    /// <summary>
    ///  All the unit tests for our PostsService
    /// </summary>
    public class PostsServiceTest
    {
        private readonly PostsService _postsService;
        private readonly Mock<IPostsRepository> _postsRepositoryMock;
        private readonly IFixture _fixture = new Fixture();

        public PostsServiceTest(ITestOutputHelper outputHelper)
        {
            _postsRepositoryMock = new Mock<IPostsRepository>();
            _postsService = new PostsService(_postsRepositoryMock.Object);
        }

        private Post CreatePost(Guid? communityId = null, Guid? userId = null, string title = "Titre", string body = "Message", DateTime? createdAt = null) =>
            new()
            {
                Id = Guid.NewGuid(), CommunityId = communityId ?? Guid.NewGuid(), UserId = userId ?? Guid.NewGuid(),
                Title = title, Body = body, CreatedAt = createdAt ?? new DateTime(2025, 1, 12), ModifiedAt = new DateTime(2025, 1, 13),
                ImagesPath = new() { "image.png" }, AdditionalsPath = new() { "document.pdf" }
            };

        #region GetAllPosts
        [Fact]
        public async Task GetAllPosts_ReturnsPostsWithReportCounts()
        {
            var posts = new List<Post> { CreatePost(), CreatePost() };
            _postsRepositoryMock.Setup(r => r.GetAllPosts()).ReturnsAsync(posts);
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int> { [posts[0].Id] = 2 });
            var result = await _postsService.GetAllPosts();
            result.Select(p => p.Id).Should().Equal(posts.Select(p => p.Id));
            result.Select(p => p.ReportsCount).Should().Equal(2, 0);
            _postsRepositoryMock.Verify(r => r.GetReportCountsByPostIds(It.Is<List<Guid>>(ids => ids.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task GetAllPosts_EmptyList_DoesNotQueryReports()
        {
            _postsRepositoryMock.Setup(r => r.GetAllPosts()).ReturnsAsync(new List<Post>());
            (await _postsService.GetAllPosts()).Should().BeEmpty();
            _postsRepositoryMock.Verify(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>()), Times.Never);
        }

        #endregion

        #region GetAllPostsCount
        [Fact]
        public async Task GetAllPostsCount_ReturnsRepositoryCount()
        {
            _postsRepositoryMock.Setup(r => r.GetPostCount()).ReturnsAsync(7);
            (await _postsService.GetAllPostsCount()).Should().Be(7);
        }
        #endregion

        #region GetCommunityFeed
        [Fact]
        public async Task GetCommunityFeed_MapsFeedItemsAndUsesFallbackNames()
        {
            var post = CreatePost();
            _postsRepositoryMock.Setup(r => r.GetCommunityFeed(post.CommunityId)).ReturnsAsync(new List<PostFeedItem>
            {
                new() { Post = post, AuthorName = null, CommunityName = null, CommunityDescription = "Info", CommunityId = post.CommunityId }
            });
            var result = await _postsService.GetCommunityFeed(post.CommunityId);
            result.Should().ContainSingle();
            result[0].Post.Id.Should().Be(post.Id);
            result[0].AuthorName.Should().Be("Membre");
            result[0].CommunityName.Should().Be("Communauté");
            result[0].CommunityDescription.Should().Be("Info");
            _postsRepositoryMock.Verify(r => r.GetCommunityFeed(post.CommunityId), Times.Once);
        }
        #endregion

        #region AddPost
        [Fact]
        public async Task AddPost_ValidRequest_CallsRepositoryAndReturnsResponse()
        {
            var request = new AddPostRequest { UserId = Guid.NewGuid(), ComunityId = Guid.NewGuid(), Title = "Nouveau", Body = "Texte" };
            _postsRepositoryMock.Setup(r => r.CommunityExists(request.ComunityId)).ReturnsAsync(true);
            _postsRepositoryMock.Setup(r => r.UserExists(request.UserId)).ReturnsAsync(true);
            _postsRepositoryMock.Setup(r => r.AddPost(It.IsAny<Post>())).ReturnsAsync((Post post) => post);
            var result = await _postsService.AddPost(request);
            result.Title.Should().Be(request.Title);
            result.Body.Should().Be(request.Body);
            result.CommunityId.Should().Be(request.ComunityId);
            _postsRepositoryMock.Verify(r => r.AddPost(It.Is<Post>(p => p.UserId == request.UserId && p.Title == request.Title)), Times.Once);
        }

        [Fact]
        public async Task AddPost_NullRequest_Throws()
        {
            await FluentActions.Invoking(() => _postsService.AddPost(null)).Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddPost_CommunityOrUserMissing_Throws()
        {
            var request = new AddPostRequest { UserId = Guid.NewGuid(), ComunityId = Guid.NewGuid(), Title = "Titre", Body = "Texte" };
            _postsRepositoryMock.Setup(r => r.CommunityExists(request.ComunityId)).ReturnsAsync(false);
            await FluentActions.Invoking(() => _postsService.AddPost(request)).Should().ThrowAsync<ArgumentException>();
            _postsRepositoryMock.Setup(r => r.CommunityExists(request.ComunityId)).ReturnsAsync(true);
            _postsRepositoryMock.Setup(r => r.UserExists(request.UserId)).ReturnsAsync(false);
            await FluentActions.Invoking(() => _postsService.AddPost(request)).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddPost_InvalidContentOrRepositoryFailure_Throws()
        {
            var request = new AddPostRequest { UserId = Guid.NewGuid(), ComunityId = Guid.NewGuid(), Title = " ", Body = "Texte" };
            await FluentActions.Invoking(() => _postsService.AddPost(request)).Should().ThrowAsync<ArgumentException>();
            request.Title = "Titre";
            _postsRepositoryMock.Setup(r => r.CommunityExists(request.ComunityId)).ReturnsAsync(true);
            _postsRepositoryMock.Setup(r => r.UserExists(request.UserId)).ReturnsAsync(true);
            _postsRepositoryMock.Setup(r => r.AddPost(It.IsAny<Post>())).ReturnsAsync((Post?)null);
            await FluentActions.Invoking(() => _postsService.AddPost(request)).Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region GetPostByPostId
        [Fact]
        public async Task GetPostByPostId_ExistingPost_ReturnsResponse()
        {
            var post = CreatePost();
            _postsRepositoryMock.Setup(r => r.GetPostByPostId(post.Id)).ReturnsAsync(post);
            (await _postsService.GetPostByPostId(post.Id)).Id.Should().Be(post.Id);
            _postsRepositoryMock.Verify(r => r.GetPostByPostId(post.Id), Times.Once);
        }

        [Fact]
        public async Task GetPostByPostId_MissingPost_Throws()
        {
            _postsRepositoryMock.Setup(r => r.GetPostByPostId(It.IsAny<Guid>())).ReturnsAsync((Post?)null);
            await FluentActions.Invoking(() => _postsService.GetPostByPostId(Guid.NewGuid())).Should().ThrowAsync<KeyNotFoundException>();
        }

        #endregion

        #region UpdatePost
        [Fact]
        public async Task UpdatePost_ValidRequest_UpdatesPostAndReturnsResponse()
        {
            var post = CreatePost();
            var request = new UpdatePostRequest { Id = post.Id, UserId = post.UserId, ComunityId = post.CommunityId, Title = "Modifié", Body = "Nouveau texte", ImagesPath = new() { "new.png" }, AdditionalsPath = new() };
            _postsRepositoryMock.Setup(r => r.GetPostByPostId(post.Id)).ReturnsAsync(post);
            _postsRepositoryMock.Setup(r => r.UpdatePost(It.IsAny<Post>())).ReturnsAsync((Post updated) => updated);
            var result = await _postsService.UpdatePost(request);
            result.Title.Should().Be("Modifié");
            result.Body.Should().Be("Nouveau texte");
            result.ImagesPath.Should().Contain("new.png");
            _postsRepositoryMock.Verify(r => r.UpdatePost(It.Is<Post>(p => p.Id == post.Id && p.Title == "Modifié")), Times.Once);
        }

        [Fact]
        public async Task UpdatePost_MissingPostOrRepositoryFailure_Throws()
        {
            var request = new UpdatePostRequest { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ComunityId = Guid.NewGuid(), Title = "T", Body = "B" };
            _postsRepositoryMock.Setup(r => r.GetPostByPostId(request.Id)).ReturnsAsync((Post?)null);
            await FluentActions.Invoking(() => _postsService.UpdatePost(request)).Should().ThrowAsync<KeyNotFoundException>();
            var post = CreatePost();
            request.Id = post.Id;
            _postsRepositoryMock.Setup(r => r.GetPostByPostId(post.Id)).ReturnsAsync(post);
            _postsRepositoryMock.Setup(r => r.UpdatePost(It.IsAny<Post>())).ReturnsAsync((Post?)null);
            await FluentActions.Invoking(() => _postsService.UpdatePost(request)).Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion

        #region DeletePostById
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task DeletePostByPostId_ReturnsWhetherRepositoryDeletedRows(int deletedRows, bool expected)
        {
            _postsRepositoryMock.Setup(r => r.DeletePostByPostId(It.IsAny<Guid>())).ReturnsAsync(deletedRows);
            (await _postsService.DeletePostByPostId(Guid.NewGuid())).Should().Be(expected);
        }


        #endregion

        #region GetAllPostsByCommunity
        [Fact]
        public async Task GetAllPostsByCommunity_ValidId_ReturnsPostsAndReportCounts()
        {
            var post = CreatePost();
            _postsRepositoryMock.Setup(r => r.GetAllPostsByComunity(post.CommunityId)).ReturnsAsync(new List<Post> { post });
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int> { [post.Id] = 4 });
            var result = await _postsService.GetAllPostsByComunity(post.CommunityId);
            result.Should().ContainSingle();
            result[0].ReportsCount.Should().Be(4);
            _postsRepositoryMock.Verify(r => r.GetAllPostsByComunity(post.CommunityId), Times.Once);
        }

        [Fact]
        public async Task GetAllPostsByCommunity_EmptyId_Throws()
        {
            await FluentActions.Invoking(() => _postsService.GetAllPostsByComunity(Guid.Empty)).Should().ThrowAsync<ArgumentException>();
            _postsRepositoryMock.Verify(r => r.GetAllPostsByComunity(It.IsAny<Guid>()), Times.Never);
        }
        #endregion

        #region GetAllFilteredPosts
        [Fact]
        public async Task GetAllFilteredPosts_EmptySearch_UsesCommunityPosts()
        {
            var post = CreatePost();
            _postsRepositoryMock.Setup(r => r.GetAllPostsByComunity(post.CommunityId)).ReturnsAsync(new List<Post> { post });
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int>());
            (await _postsService.GetAllFilteredPostsByComunitiy(post.CommunityId, "", "")).Should().ContainSingle();
            _postsRepositoryMock.Verify(r => r.GetAllPostsByComunity(post.CommunityId), Times.Once);
        }

        [Fact]
        public async Task GetAllFilteredPosts_CreatedAtSearch_FiltersInMemory()
        {
            var community = Guid.NewGuid();
            var matching = CreatePost(community, createdAt: new DateTime(2025, 1, 12));
            var other = CreatePost(community, createdAt: new DateTime(2024, 2, 12));
            _postsRepositoryMock.Setup(r => r.GetAllPostsByComunity(community)).ReturnsAsync(new List<Post> { matching, other });
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int>());
            var dateSearch = matching.CreatedAt.ToString("MMM yyyy");
            var result = await _postsService.GetAllFilteredPostsByComunitiy(community, nameof(PostResponse.CreatedAt), dateSearch);
            result.Should().ContainSingle().Which.Id.Should().Be(matching.Id);
            _postsRepositoryMock.Verify(r => r.GetAllFilteredPostsByComunitiy(It.IsAny<Expression<Func<Post, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetAllFilteredPosts_OtherField_PassesMatchingPredicateToRepository()
        {
            var community = Guid.NewGuid();
            var post = CreatePost(community, title: "Bonjour monde");
            _postsRepositoryMock.Setup(r => r.GetAllFilteredPostsByComunitiy(It.IsAny<Expression<Func<Post, bool>>>()))
                .Returns((Expression<Func<Post, bool>> predicate) => Task.FromResult(new[] { post }.AsQueryable().Where(predicate).ToList()));
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int>());
            var result = await _postsService.GetAllFilteredPostsByComunitiy(community, nameof(PostResponse.Title), "BONJ");
            result.Should().ContainSingle().Which.Id.Should().Be(post.Id);
            _postsRepositoryMock.Verify(r => r.GetAllFilteredPostsByComunitiy(It.IsAny<Expression<Func<Post, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllFilteredPosts_EmptyCommunity_Throws()
        {
            await FluentActions.Invoking(() => _postsService.GetAllFilteredPostsByComunitiy(Guid.Empty, "Title", "abc")).Should().ThrowAsync<ArgumentException>();
        }
        #endregion

        #region GetAllSortedPosts
        [Fact]
        public async Task GetAllSortedPosts_SortsSupportedFieldsInBothDirections()
        {
            var first = new PostResponse { Title = "B", Body = "2", UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"), CreatedAt = new DateTime(2025, 2, 1) };
            var second = new PostResponse { Title = "A", Body = "1", UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), CreatedAt = new DateTime(2025, 1, 1) };
            var posts = new List<PostResponse> { first, second };
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, nameof(PostResponse.Title), SortOption.ASC)).Select(p => p.Title).Should().Equal("A", "B");
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, nameof(PostResponse.Body), SortOption.DESC)).Select(p => p.Body).Should().Equal("2", "1");
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, nameof(PostResponse.UserId), SortOption.ASC)).First().Should().BeSameAs(second);
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, nameof(PostResponse.CreatedAt), SortOption.DESC)).First().Should().BeSameAs(first);
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, "Inconnu", SortOption.ASC)).Should().BeSameAs(posts);
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), posts, "", SortOption.ASC)).Should().BeSameAs(posts);
            (await _postsService.GetAllSortedPostsByComunity(Guid.NewGuid(), null!, nameof(PostResponse.Title), SortOption.ASC)).Should().BeEmpty();
        }
        #endregion

        #region ExportsData
        [Fact]
        public async Task GetPostsCSV_ReturnsCsvContent()
        {
            var post = CreatePost(title: "Titre CSV");
            _postsRepositoryMock.Setup(r => r.GetAllPosts()).ReturnsAsync(new List<Post> { post });
            _postsRepositoryMock.Setup(r => r.GetReportCountsByPostIds(It.IsAny<List<Guid>>())).ReturnsAsync(new Dictionary<Guid, int>());
            using var stream = await _postsService.GetPostsCSV();
            stream.Position.Should().Be(0);
            using var reader = new StreamReader(stream, leaveOpen: true);
            (await reader.ReadToEndAsync()).Should().Contain("Titre CSV");
        }

        [Fact]
        public async Task GetPostsEXCEL_ReturnsNonEmptyWorkbook()
        {
            _postsRepositoryMock.Setup(r => r.GetAllPosts()).ReturnsAsync(new List<Post> { CreatePost() });
            using var stream = await _postsService.GetPostsEXCEL();
            stream.Position.Should().Be(0);
            stream.Length.Should().BeGreaterThan(0);
            _postsRepositoryMock.Verify(r => r.GetAllPosts(), Times.Once);
        }
        #endregion
    }
}
