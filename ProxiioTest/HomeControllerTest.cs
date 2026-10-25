using CRUDCoursesApp.Controllers;
using CRUDCoursesApp.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;

namespace ProxiioTest;

/// <summary>
///  All the  tests for our HomeController will be found down here
/// </summary>
public class HomeControllerTest
{
    private readonly Mock<IUsersService> _users = new();
    private readonly Mock<IComunitiesService> _communities = new();
    private readonly Mock<IPostsService> _posts = new();
    private readonly HomeController _controller;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _communityId = Guid.NewGuid();

    public HomeControllerTest()
    {
        _controller = new HomeController(_users.Object, _communities.Object, _posts.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsModelWithAlphabeticallyFirstUserCommunitiesAndFeed()
    {
        var feed = new List<FeedPostResponse> { new() { CommunityId = _communityId, CommunityName = "Community" } };
        _users.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<UserResponse>
        {
            new() { UserId = Guid.NewGuid(), Name = "Zoé" }, new() { UserId = _userId, Name = "Alice" }
        });
        _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(new List<ComunityResponse> { new() { Id = _communityId, Name = "Community", Description = "Description" } });
        _posts.Setup(s => s.GetCommunityFeed()).ReturnsAsync(feed);

        var view = (await _controller.Index(null)).Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<CommunityHomeViewModel>().Subject;
        model.CurrentUserId.Should().Be(_userId);
        model.CurrentUserName.Should().Be("Alice");
        model.Communities.Should().ContainSingle().Which.Id.Should().Be(_communityId);
        model.Feed.Should().BeSameAs(feed);
        _posts.Verify(s => s.GetCommunityFeed(), Times.Once);
    }

    [Fact]
    public async Task Index_SearchFiltersCommunityNameOrDescriptionIgnoringCase()
    {
        _users.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<UserResponse>());
        _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(new List<ComunityResponse>
        {
            new() { Name = "Gardening", Description = "Outside" },
            new() { Name = "Cooking", Description = "Gardening advice" },
            new() { Name = "Music", Description = "Songs" }
        });
        _posts.Setup(s => s.GetCommunityFeed()).ReturnsAsync(new List<FeedPostResponse>());

        var view = (await _controller.Index("GARDEN")).Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<CommunityHomeViewModel>().Subject;
        model.CurrentUserId.Should().Be(Guid.Empty);
        model.CurrentUserName.Should().Be("Membre");
        model.Communities.Select(c => c.Name).Should().BeEquivalentTo("Gardening", "Cooking");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Index_BlankSearchLeavesAllCommunities(string? query)
    {
        _users.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<UserResponse>());
        _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(new List<ComunityResponse> { new() { Name = "A" }, new() { Name = "B" } });
        _posts.Setup(s => s.GetCommunityFeed()).ReturnsAsync(new List<FeedPostResponse>());
        var view = (await _controller.Index(query)).Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().BeOfType<CommunityHomeViewModel>().Which.Communities.Should().HaveCount(2);
    }
}
