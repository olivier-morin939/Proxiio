using CRUDCoursesApp.Controllers;
using CRUDCoursesApp.ViewModels;
using Entities.Enums;
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
///  All the  tests for our CommunitiesController will be found down here
/// </summary>
public class CommunitiesControllerTest
{
    private readonly Mock<IUsersService> _users = new();
    private readonly Mock<IPostsService> _posts = new();
    private readonly Mock<IComunitiesService> _communities = new();
    private readonly Mock<IComunityMembersService> _members = new();
    private readonly CommunitiesController _controller;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _communityId = Guid.NewGuid();
    private UserResponse User => new() { UserId = _userId, Name = "Alice", Email = "alice@example.com" };
    private ComunityResponse Community => new() { Id = _communityId, TeacherId = _userId, Name = "Community", Users = new() { User } };

    public CommunitiesControllerTest()
    {
        _controller = new CommunitiesController(_users.Object, _posts.Object, _communities.Object, _members.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
        _users.Setup(s => s.GetAllUsers()).ReturnsAsync(() => new List<UserResponse> { User });
        _communities.Setup(s => s.GetComunityByComId(It.IsAny<Guid>())).ReturnsAsync(() => Community);
        _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(() => new List<ComunityResponse> { Community });
        _communities.Setup(s => s.AddComunity(It.IsAny<AddComunityRequest?>())).ReturnsAsync(() => Community);
        _members.Setup(s => s.GetComunityMembersCount(It.IsAny<Guid>())).ReturnsAsync(1);
        _members.Setup(s => s.IsComunityMember(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
        _members.Setup(s => s.AddComunityMember(It.IsAny<AddComunityMemberRequest?>())).ReturnsAsync(new ComunityMemberResponse { ComunityId = _communityId, UserId = _userId });
        _posts.Setup(s => s.GetCommunityFeed(It.IsAny<Guid?>())).ReturnsAsync(new List<FeedPostResponse>());
        _posts.Setup(s => s.AddPost(It.IsAny<AddPostRequest?>())).ReturnsAsync(new PostResponse { Id = Guid.NewGuid() });
    }

    [Fact]
    public async Task Details_ExistingCommunity_ReturnsViewModelAndMemberState()
    {
        var result = (await _controller.Details(_communityId)).Should().BeOfType<ViewResult>().Subject;
        var model = result.Model.Should().BeOfType<CommunityDetailViewModel>().Subject;
        model.Community.Id.Should().Be(_communityId);
        model.CurrentUserName.Should().Be("Alice");
        int memberCount = (int)_controller.ViewBag.MemberCount;
        bool isMember = (bool)_controller.ViewBag.IsMember;
        memberCount.Should().Be(1);
        isMember.Should().BeTrue();
        _posts.Verify(s => s.GetCommunityFeed(_communityId), Times.Once);
    }

    [Fact]
    public async Task Details_MissingCommunityOrArgumentException_ReturnsNotFound()
    {
        _communities.Setup(s => s.GetComunityByComId(_communityId)).ReturnsAsync((ComunityResponse?)null);
        (await _controller.Details(_communityId)).Should().BeOfType<NotFoundResult>();
        _communities.Setup(s => s.GetComunityByComId(_communityId)).ThrowsAsync(new ArgumentNullException());
        (await _controller.Details(_communityId)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidInput_TrimsValuesAndUsesAlphabeticallyFirstUser()
    {
        _users.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<UserResponse> { new() { UserId = Guid.NewGuid(), Name = "Zoé" }, User });
        var result = (await _controller.Create("  New community  ", "  Description  ")).Should().BeOfType<RedirectToActionResult>().Subject;
        result.ActionName.Should().Be("Details");
        _communities.Verify(s => s.AddComunity(It.Is<AddComunityRequest>(r => r.TeacherId == _userId && r.Name == "New community" && r.Description == "Description")), Times.Once);
    }

    [Theory]
    [InlineData(" ", "description")]
    [InlineData("", "")]
    public async Task Create_InvalidInput_RedirectsHome(string name, string description)
    {
        var result = (await _controller.Create(name, description)).Should().BeOfType<RedirectToActionResult>().Subject;
        result.ActionName.Should().Be("Index");
        _controller.TempData["Error"].Should().NotBeNull();
        _communities.Verify(s => s.AddComunity(It.IsAny<AddComunityRequest?>()), Times.Never);
    }

    [Fact]
    public async Task Create_OverLimitInput_IsRejected()
    {
        (await _controller.Create(new string('x', 121), "")).Should().BeOfType<RedirectToActionResult>();
        (await _controller.Create("Name", new string('x', 255))).Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Join_CommunityMissing_ReturnsNotFound()
    {
        _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(new List<ComunityResponse>());
        (await _controller.Join(_communityId)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Join_ExistingMember_IsNotAddedTwice()
    {
        _members.Setup(s => s.IsComunityMember(_communityId, _userId)).ReturnsAsync(true);
        var result = (await _controller.Join(_communityId)).Should().BeOfType<RedirectToActionResult>().Subject;
        result.ActionName.Should().Be("Details");
        _members.Verify(s => s.AddComunityMember(It.IsAny<AddComunityMemberRequest?>()), Times.Never);
    }

    [Fact]
    public async Task Join_NonMember_AddsMember()
    {
        _members.Setup(s => s.IsComunityMember(_communityId, _userId)).ReturnsAsync(false);
        (await _controller.Join(_communityId)).Should().BeOfType<RedirectToActionResult>();
        _members.Verify(s => s.AddComunityMember(It.Is<AddComunityMemberRequest>(r => r.ComunityId == _communityId && r.UserId == _userId && r.Role == ComunityRole.Member)), Times.Once);
    }

    [Theory]
    [InlineData(" ", "body")]
    [InlineData("title", " ")]
    public async Task Post_EmptyTitleOrBody_RedirectsWithoutCreatingPost(string title, string body)
    {
        (await _controller.Post(_communityId, title, body)).Should().BeOfType<RedirectToActionResult>();
        _posts.Verify(s => s.AddPost(It.IsAny<AddPostRequest?>()), Times.Never);
    }

    [Fact]
    public async Task Post_OverLimitTitleOrBody_IsRejected()
    {
        (await _controller.Post(_communityId, new string('x', 121), "body")).Should().BeOfType<RedirectToActionResult>();
        (await _controller.Post(_communityId, "title", new string('x', 251))).Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Post_UserNotMember_ReturnsForbidden()
    {
        _members.Setup(s => s.IsComunityMember(_communityId, _userId)).ReturnsAsync(false);
        (await _controller.Post(_communityId, "Title", "Body")).Should().BeOfType<ForbidResult>();
        _posts.Verify(s => s.AddPost(It.IsAny<AddPostRequest?>()), Times.Never);
    }

    [Fact]
    public async Task Post_Member_AddsTrimmedPostToCommunity()
    {
        var result = (await _controller.Post(_communityId, " Title ", " Body ")).Should().BeOfType<RedirectToActionResult>().Subject;
        result.ActionName.Should().Be("Details");
        _posts.Verify(s => s.AddPost(It.Is<AddPostRequest>(r => r.ComunityId == _communityId && r.UserId == _userId && r.Title == "Title" && r.Body == "Body")), Times.Once);
    }
}

