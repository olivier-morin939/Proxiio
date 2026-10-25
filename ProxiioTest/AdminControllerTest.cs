using CRUDCoursesApp.Controllers;
using Entities.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Reports;
using ServiceContracts.DTO.Signals;
using ServiceContracts.DTO.Users;
using System.Data;

namespace ProxiioTest
{
    /// <summary>
    ///  All the  tests for our AdminController will be found down here
    /// </summary>
    public class AdminControllerTest
    {
        private readonly Mock<IUsersService> _users = new();
        private readonly Mock<IComunitiesService> _communities = new();
        private readonly Mock<IComunityMembersService> _members = new();
        private readonly Mock<IPostsService> _posts = new();
        private readonly Mock<IReportsService> _reports = new();
        private readonly Mock<ISignalsService> _signals = new();
        private readonly Mock<ILogger<AdminController>> _logger = new();
        private readonly AdminController _controller;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _communityId = Guid.NewGuid();
        private readonly Guid _postId = Guid.NewGuid();
        private readonly Guid _signalId = Guid.NewGuid();
        private readonly Guid _reportId = Guid.NewGuid();

        private UserResponse User => new() { UserId = _userId, Name = "Alice", Email = "alice@example.com", Password = "Password123!", DateOfBirth = new DateTime(1990, 1, 1) };
        private ComunityResponse Community => new() { Id = _communityId, TeacherId = _userId, Name = "Communauté", Description = "Description", Users = new() { User } };
        private PostResponse Post => new() { Id = _postId, CommunityId = _communityId, UserId = _userId, Title = "Publication", Body = "Texte" };
        private SignalResponse Signal => new() { Id = _signalId, ProblemName = "Erreur" };
        private ReportResponse Report => new() { Id = _reportId, PostId = _postId, MessageOfReport = "Signalement", StatusOfReport = ReportStatus.Received };

        public AdminControllerTest()
        {
            SetupServices();
            _controller = new AdminController(_users.Object, _communities.Object, _members.Object, _posts.Object, _reports.Object, _signals.Object, _logger.Object);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
        }

        private void SetupServices()
        {
            _users.Setup(s => s.GetAllUsersCount()).ReturnsAsync(1);
            _users.Setup(s => s.GetAllUsers()).ReturnsAsync(() => new List<UserResponse> { User });
            _users.Setup(s => s.GetFilteredUsers(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => new List<UserResponse> { User });
            _users.Setup(s => s.GetSortedUsers(It.IsAny<List<UserResponse>>(), It.IsAny<string>(), It.IsAny<SortOption>())).Returns((List<UserResponse> list, string _, SortOption _) => Task.FromResult(list));
            _users.Setup(s => s.GetUserById(It.IsAny<Guid>())).ReturnsAsync(() => User);
            _users.Setup(s => s.AddUser(It.IsAny<AddUserRequest?>())).ReturnsAsync(() => User);
            _users.Setup(s => s.UpdateUser(It.IsAny<UpdateUserRequest?>())).ReturnsAsync(() => User);
            _users.Setup(s => s.DeleteUser(It.IsAny<Guid>())).ReturnsAsync(true);
            _users.Setup(s => s.UploadUsersFromExcel(It.IsAny<IFormFile>())).ReturnsAsync(1);
            _users.Setup(s => s.GetUsersCSV()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
            _users.Setup(s => s.GetUsersEXCEL()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _communities.Setup(s => s.GetAllComunitiesCount()).ReturnsAsync(1);
            _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(() => new List<ComunityResponse> { Community });
            _communities.Setup(s => s.GetFilteredComunities(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => new List<ComunityResponse> { Community });
            _communities.Setup(s => s.GetSortedComunities(It.IsAny<List<ComunityResponse>>(), It.IsAny<string>(), It.IsAny<SortOption>())).Returns((List<ComunityResponse> list, string _, SortOption _) => Task.FromResult(list));
            _communities.Setup(s => s.GetComunityByComId(It.IsAny<Guid>())).ReturnsAsync(() => Community);
            _communities.Setup(s => s.AddComunity(It.IsAny<AddComunityRequest?>())).ReturnsAsync(() => Community);
            _communities.Setup(s => s.UpdateComunity(It.IsAny<UpdateComunityRequest?>())).ReturnsAsync(() => Community);
            _communities.Setup(s => s.DeleteComunityByComId(It.IsAny<Guid>())).ReturnsAsync(true);
            _communities.Setup(s => s.GetComCSV()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
            _communities.Setup(s => s.GetComEXCEL()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _members.Setup(s => s.GetComunityMembers(It.IsAny<Guid>())).ReturnsAsync(() => new List<ComunityMemberResponse> { new() { ComunityId = _communityId, UserId = _userId } });
            _members.Setup(s => s.AddComunityMember(It.IsAny<AddComunityMemberRequest?>())).ReturnsAsync(new ComunityMemberResponse { ComunityId = _communityId, UserId = _userId });
            _members.Setup(s => s.RemoveComunityMember(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            _posts.Setup(s => s.GetAllPostsCount()).ReturnsAsync(1);
            _posts.Setup(s => s.GetAllPosts()).ReturnsAsync(() => new List<PostResponse> { Post });
            _posts.Setup(s => s.GetAllFilteredPostsByComunitiy(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => new List<PostResponse> { Post });
            _posts.Setup(s => s.GetAllSortedPostsByComunity(It.IsAny<Guid>(), It.IsAny<List<PostResponse>>(), It.IsAny<string>(), It.IsAny<SortOption>())).Returns((Guid _, List<PostResponse> list, string _, SortOption _) => Task.FromResult(list));
            _posts.Setup(s => s.AddPost(It.IsAny<AddPostRequest?>())).ReturnsAsync(() => Post);
            _posts.Setup(s => s.DeletePostByPostId(It.IsAny<Guid>())).ReturnsAsync(true);
            _posts.Setup(s => s.GetPostByPostId(It.IsAny<Guid>())).ReturnsAsync(() => Post);
            _posts.Setup(s => s.UpdatePost(It.IsAny<UpdatePostRequest>())).ReturnsAsync(() => Post);
            _posts.Setup(s => s.GetPostsCSV()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
            _posts.Setup(s => s.GetPostsEXCEL()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _signals.Setup(s => s.GetAllSignals()).ReturnsAsync(() => new List<SignalResponse> { Signal });
            _signals.Setup(s => s.GetSignalById(It.IsAny<Guid>())).ReturnsAsync(() => Signal);
            _signals.Setup(s => s.AddSignal(It.IsAny<AddSignalRequest>())).ReturnsAsync(() => Signal);
            _signals.Setup(s => s.UpdateSignal(It.IsAny<UpdateSignalRequest>())).ReturnsAsync(() => Signal);
            _signals.Setup(s => s.DeleteSignal(It.IsAny<Guid>())).ReturnsAsync(true);
            _signals.Setup(s => s.GetSignalsCSV()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
            _signals.Setup(s => s.GetSignalsEXCEL()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _reports.Setup(s => s.GetAllReports()).ReturnsAsync(() => new List<ReportResponse> { Report });
            _reports.Setup(s => s.GetReportByReportId(It.IsAny<Guid>())).ReturnsAsync(() => Report);
            _reports.Setup(s => s.AddReport(It.IsAny<AddReportRequest?>())).ReturnsAsync(() => Report);
            _reports.Setup(s => s.UpdateReport(It.IsAny<UpdateReportRequest?>())).ReturnsAsync(() => Report);
            _reports.Setup(s => s.DeleteReportByReportId(It.IsAny<Guid>())).ReturnsAsync(true);
            _reports.Setup(s => s.GetReportsCSV()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
            _reports.Setup(s => s.GetReportsEXCEL()).ReturnsAsync(new MemoryStream(new byte[] { 1 }));
        }

        private static void IsView(IActionResult? result, string viewName) =>
            result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be(viewName);

        private static void IsRedirect(IActionResult? result, string actionName) =>
            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(actionName);

        [Fact]
        public async Task DashboardAndListActions_ReturnViewsAndDelegateToServices()
        {
            IsView(await _controller.Index(), null!);
            int userCount = (int)_controller.ViewBag.TotalUserCounts;
            int signalCount = (int)_controller.ViewBag.TotalSignalsCounts;
            userCount.Should().Be(1);
            signalCount.Should().Be(1);

            IsView(await _controller.DisplayUsers("Name", "Ali"), "User/DisplayUsers");
            IsView(await _controller.DisplayComunities("Name", "Comm"), "Comunity/DisplayComunities");
            IsView(await _controller.DisplayPosts(), "Post/DisplayPostsByComunity");
            IsView(await _controller.DisplaySignals(), "Signal/DisplaySignals");
            IsView(await _controller.DisplayReports(), "Report/DisplayReports");
            IsView(await _controller.DisplayReports(_postId), "Report/DisplayReports");
            ((List<ReportResponse>)((ViewResult)(await _controller.DisplayReports(_postId))).Model!).Should().OnlyContain(r => r.PostId == _postId);
        }

        [Fact]
        public async Task UserActions_CoversGetPostAndErrorPaths()
        {
            IsView(await _controller.AddUser(), "User/AddUser");
            IsView(await _controller.UploadUsersFromExcel(), "User/UploadUsersFromExcel");
            IsView(await _controller.DisplaySpecificUser(_userId), "User/DisplaySpecificUser");
            IsView(await _controller.UpdateSpecificUser(_userId), "User/UpdateSpecificUser");
            IsView(await _controller.DeleteSpecificUser(_userId), "User/DeleteSpecificUser");

            var add = new AddUserRequest { Name = "Alice", Email = "alice@example.com", Password = "Password123!", ConfirmPassword = "Password123!", DateOfBirth = new DateTime(1990, 1, 1) };
            IsRedirect(await _controller.AddUser(add), "DisplayUsers");
            _controller.ModelState.AddModelError("Name", "invalid");
            IsRedirect(await _controller.AddUser(add), "AddUser");
            _controller.ModelState.Clear();
            IsRedirect(await _controller.UpdateSpecificUser(new UpdateUserRequest(), _userId), "DisplayUsers");
            IsRedirect(await _controller.DeleteSpecificUserConfirm(_userId), "DisplayUsers");

            var formFile = new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "file", "users.xlsx");
            IsView(await _controller.UploadUsersFromExcel(formFile), "User/UploadUsersFromExcel");
            IsView(await _controller.UploadUsersFromExcel(new FormFile(Stream.Null, 0, 0, "file", "users.csv")), "User/UploadUsersFromExcel");

            _users.Setup(s => s.GetUserById(_userId)).ReturnsAsync((UserResponse?)null);
            (await _controller.DisplaySpecificUser(_userId)).Should().BeOfType<NotFoundResult>();
            (await _controller.UpdateSpecificUser(_userId)).Should().BeOfType<NotFoundResult>();
            (await _controller.DeleteSpecificUser(_userId)).Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CommunityActions_CoversViewsPostsAndMemberFlows()
        {
            IsView(await _controller.AddComunity(), "Comunity/AddComunity");
            IsView(await _controller.UpdateComunity(_communityId), "Comunity/UpdateComunity");
            IsView(await _controller.DeleteSpecificComunity(_communityId), "Comunity/DeleteSpecificComunity");
            IsView(await _controller.DisplayUserByComunity(_communityId), "Comunity/DisplayUserByComunity");
            IsView(await _controller.AddUserByComunity(_communityId), "Comunity/AddUserByComunity");
            IsView(await _controller.ConfirmDeleteUserByComunity(_communityId, _userId), "Comunity/DeleteUserByComunity");

            IsRedirect(await _controller.AddComunity(new AddComunityRequest { TeacherId = _userId, Name = "New" }), "DisplayComunities");
            IsRedirect(await _controller.UpdateSpecificComunity(new UpdateComunityRequest { Name = "New", TeacherId = _userId }, _communityId), "DisplaySpecificComunity");
            IsRedirect(await _controller.DeleteSpecificComunityConfirm(_communityId), "DisplayComunities");
            IsRedirect(await _controller.AddUserByComunity(_communityId, new AddUserRequest { Name = "Bob", Email = "bob@example.com", Password = "Password123!", ConfirmPassword = "Password123!", DateOfBirth = new DateTime(1990, 1, 1) }), "DisplayUserByComunity");
            IsRedirect(await _controller.DeleteUserByComunity(_communityId, _userId), "DisplayUserByComunity");
            IsView(await _controller.DisplaySpecificComunity(_communityId), "Comunity/DisplaySpecificComunity");
            IsView(await _controller.DisplayPostsByComunity(_communityId, "", ""), "Post/DisplayPostsByComunity");

            _communities.Setup(s => s.GetAllComunities()).ReturnsAsync(new List<ComunityResponse>());
            (await _controller.AddUserByComunity(_communityId)).Should().BeOfType<NotFoundResult>();
            _communities.Setup(s => s.GetComunityByComId(_communityId)).ReturnsAsync((ComunityResponse?)null);
            (await _controller.UpdateComunity(_communityId)).Should().BeOfType<NotFoundResult>();
            (await _controller.DeleteSpecificComunity(_communityId)).Should().BeOfType<NotFoundResult>();
            (await _controller.DisplayUserByComunity(_communityId)).Should().BeOfType<NotFoundResult>();
            (await _controller.DisplaySpecificComunity(_communityId)).Should().BeOfType<NotFoundResult>();
            (await _controller.DisplayPostsByComunity(_communityId, "", "")).Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CommunityPostActions_CoversAddUpdateDeleteAndList()
        {
            IsView(await _controller.AddPostByComunity(_communityId), "Post/AddPostByComunity");
            IsView(await _controller.UpdatePostByComunity(_communityId, _postId), "Post/UpdatePostByComunity");
            IsRedirect(await _controller.AddPostByComunity(_communityId, new AddPostRequest { UserId = _userId, Title = "Titre", Body = "Texte" }), "DisplayPostsByComunity");
            IsRedirect(await _controller.DeletePostsByComunity(_communityId, _postId), "DisplayPostsByComunity");
            IsRedirect(await _controller.UpdatePostByComunity(_communityId, _postId, new UpdatePostRequest { Id = _postId, UserId = Guid.Empty, ComunityId = Guid.Empty, Title = "Titre", Body = "Texte" }), "DisplayPostsByComunity");
            _posts.Verify(s => s.AddPost(It.Is<AddPostRequest>(r => r.ComunityId == _communityId)), Times.Once);
            _posts.Verify(s => s.UpdatePost(It.Is<UpdatePostRequest>(r => r.UserId == _postId && r.ComunityId == _communityId)), Times.Once);
        }

        [Fact]
        public async Task SignalAndReportActions_CoversGetPostAndStatusFlows()
        {
            IsView(await _controller.AddSignal(), "Signal/AddSignal");
            IsView(await _controller.UpdateSignal(_signalId), "Signal/UpdateSignal");
            IsRedirect(await _controller.AddSignal(new AddSignalRequest { ProblemName = "Erreur" }), "DisplaySignals");
            IsRedirect(await _controller.UpdateSignal(_signalId, new UpdateSignalRequest { ProblemName = "Erreur" }), "DisplaySignals");
            IsRedirect(await _controller.DeleteSignal(_signalId), "DisplaySignals");

            IsView(await _controller.AddReport(), "Report/AddReport");
            IsRedirect(await _controller.AddReport(new AddReportRequest { PostId = _postId, MessageOfReport = "Spam" }), "DisplayReports");
            IsRedirect(await _controller.UpdateReportStatus(_reportId, ReportStatus.Terminated), "DisplayReports");
            IsRedirect(await _controller.DeleteReport(_reportId), "DisplayReports");
            _reports.Verify(s => s.UpdateReport(It.Is<UpdateReportRequest>(r => r.Id == _reportId && r.StatusOfReport == ReportStatus.Terminated)), Times.Once);
        }

        [Theory]
        [InlineData("ExportToPDF", "ExportToPDF")]
        [InlineData("ExportToCSV", "ExportToCSV")]
        [InlineData("ExportToEXCEL", "ExportToEXCEL")]
        public async Task HandleExports_RecognizedTypes_Redirects(string type, string expectedAction)
        {
            var result = await _controller.HandleExports("UserResponse", type);
            IsRedirect(result, expectedAction);
        }

        [Fact]
        public async Task HandleExports_RequiresKnownType()
        {
            (await _controller.HandleExports("UserResponse", "")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.HandleExports("UserResponse", "unknown")).Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("UserResponse")]
        [InlineData("ComunityResponse")]
        [InlineData("PostResponse")]
        [InlineData("ReportResponse")]
        [InlineData("SignalResponse")]
        public async Task ExportToPDF_KnownModels_ReturnPdf(string model)
        {
            (await _controller.ExportToPDF(model)).Should().BeOfType<Rotativa.AspNetCore.ViewAsPdf>();
        }

        [Theory]
        [InlineData("UserResponse", "Users.csv")]
        [InlineData("ComunityResponse", "Communities.csv")]
        [InlineData("PostResponse", "Posts.csv")]
        [InlineData("ReportResponse", "Reports.csv")]
        [InlineData("SignalResponse", "Signals.csv")]
        public async Task ExportToCSV_KnownModels_ReturnFile(string model, string fileName)
        {
            var result = (await _controller.ExportToCSV(model)).Should().BeOfType<FileStreamResult>().Subject;
            result.FileDownloadName.Should().Be(fileName);
        }

        [Theory]
        [InlineData("UserResponse", "users.xlsx")]
        [InlineData("ComunityResponse", "communities.xlsx")]
        [InlineData("PostResponse", "posts.xlsx")]
        [InlineData("ReportResponse", "reports.xlsx")]
        [InlineData("SignalResponse", "signals.xlsx")]
        public async Task ExportToEXCEL_KnownModels_ReturnFile(string model, string fileName)
        {
            var result = (await _controller.ExportToEXCEL(model)).Should().BeOfType<FileStreamResult>().Subject;
            result.FileDownloadName.Should().Be(fileName);
        }

        [Fact]
        public async Task ExportActions_RejectMissingAndUnknownModels()
        {
            (await _controller.ExportToPDF("")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.ExportToPDF("Nope")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.ExportToCSV(" ")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.ExportToCSV("Nope")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.ExportToEXCEL("")).Should().BeOfType<BadRequestObjectResult>();
            (await _controller.ExportToEXCEL("Nope")).Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
