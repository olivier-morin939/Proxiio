using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Reports;
using ServiceContracts.DTO.Users;
using Services;
using System.ComponentModel;
using System.Data;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our ReportsService
    /// </summary>
    public class ReportsServiceTest
    {
        private readonly IComunitiesService _comunitiesService;
        private readonly IUsersService _usersService;
        private readonly IPostsService _postsService;
        private readonly IReportsService _reportsService;
        private readonly ITestOutputHelper _outputHelper;
        public ReportsServiceTest(ITestOutputHelper outputHelper)
        {
            _comunitiesService = new ComunitiesService();
            _usersService = new UsersService();
            _postsService = new PostsService();
            _reportsService = new ReportsService();
            _outputHelper = outputHelper;
        }

        public AddUserRequest AddBasicUserRequest(string Name = "John Smith",string Email = "johnsmith1234@gmail.com",string Password = "Password1234!",string ConfirmPassword = "Password1234!",Role Role = Role.User,UserState UserState = UserState.Active,string DateOfBirthStr = "2000-08-06",bool ReceiveNewsLetter = true)
        {
            return new AddUserRequest()
            {
                Name = Name,
                Email = Email,
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                Role = Role,
                UserState = UserState,
                DateOfBirth = DateTime.Parse(DateOfBirthStr),
                ReceiveNewsLetter = ReceiveNewsLetter
            };
        }


        public AddPostRequest AddBasicPostRequest(Guid UserId,Guid ComunityId,string Title = "Testing post title",string Body = "Testing post body")
        {
            List<string> ImagesPaths = new List<string>();
            List<string> AdditionalsPath = new List<string>();


            return new AddPostRequest()
            {
                UserId = UserId,
                ComunityId = ComunityId,
                Title = Title,
                Body = Body,
                ImagesPath = ImagesPaths,
                AdditionalsPath = AdditionalsPath

            };
        }


        public AddReportRequest AddBasicReportRequest(Guid postId,ReportStatus statusOfReport = ReportStatus.Received,ReportType typeOfReport = ReportType.HATE_SPEECH,string messageOfReport = "default message of report")
        {
            return new AddReportRequest()
            {
                PostId = postId,
                StatusOfReport = statusOfReport,
                TypeOfReport = typeOfReport,
                MessageOfReport = messageOfReport
            };
        }


        public AddComunityRequest AddBasicComunityRequest(Guid TeacherId,string Name = "Testing community",string Description = "Mocking description") 
        {
            List<PostResponse> Posts = new List<PostResponse>();
            return new AddComunityRequest()
            {
                Name = Name,
                Description = Description,
                Posts = Posts,
                TeacherId = TeacherId

            };
        
        }


        public List<PostResponse> AddAllRequests()
        {
            // Add some users
            List<AddUserRequest> add_user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email: "testing1234@gmail.com"),
                AddBasicUserRequest(Email: "testing5555@gmail.com"),
                AddBasicUserRequest(Email: "testing6666@gmail.com"),
                AddBasicUserRequest(Email: "testing8767@gmail.com")

            };
            List<UserResponse> user_responses_from_add = new List<UserResponse>();

            foreach (AddUserRequest addUserRequest in add_user_requests)
            {
                user_responses_from_add.Add(_usersService.AddUser(addUserRequest));
            }

            // Add some communities
            List<AddComunityRequest> add_com_requests = new List<AddComunityRequest>()
            {
                AddBasicComunityRequest(TeacherId: user_responses_from_add[2].UserId),
            };
            List<ComunityResponse> com_responses_from_add = new List<ComunityResponse>();

            foreach (AddComunityRequest addComRequest in add_com_requests)
            {
                com_responses_from_add.Add(_comunitiesService.AddComunity(addComRequest));
            }


            // Add some posts
            List<AddPostRequest> add_post_requests = new List<AddPostRequest>()
            {
                AddBasicPostRequest(UserId:user_responses_from_add[0].UserId, ComunityId: com_responses_from_add[0].Id)

            };
            List<PostResponse> post_responses_from_add = new List<PostResponse>();

            foreach (AddPostRequest addPostRequest in add_post_requests)
            {
                post_responses_from_add.Add(_postsService.AddPost(addPostRequest));
            }

            return post_responses_from_add;
        }


        #region AddReport
        [Fact]
        public void AddReport_ReportIsNull()
        {
            //Arrange
            List<PostResponse> postResponses = AddAllRequests();
            AddReportRequest? null_report_request = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => {
                
                _reportsService.AddReport(null_report_request);
            });

        }

        [Fact]
        public void AddReport_InvalidReportProps()
        {
            //Arrange
            // Add some users
            List<PostResponse> postResponses = AddAllRequests();

            List<AddReportRequest> invalid_report_requests = new List<AddReportRequest>()
            {
                AddBasicReportRequest(
                    postId: postResponses[0].Id, 
                    messageOfReport : @"TOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGG
                                        TOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGG
                                        TOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGG
                                        TOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGGTOOOLONNNGG"

                    ),
                AddBasicReportRequest(postId:postResponses[0].Id, messageOfReport: string.Empty),
            };

            // Assert
            foreach(AddReportRequest addReportRequest in invalid_report_requests)
            {
                Assert.Throws<ArgumentException>(() => {

                    _reportsService.AddReport(addReportRequest);
                });
            };
            
        }


        [Fact]
        public void AddReport_ValidReport()
        {
            //Arrange
            List<PostResponse> postResponses = AddAllRequests();

            _outputHelper.WriteLine($"{postResponses[0].ToString()}");

            List<AddReportRequest> valid_report_requests = new List<AddReportRequest>()
            {

                AddBasicReportRequest(postId: postResponses[0].Id)
            };

            //Act
            ReportResponse expected_valid_report_response = _reportsService.AddReport(valid_report_requests[0]);
            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_valid_report_response.ToString()}");

            ReportResponse actual_valid_report_response = _reportsService.GetReportByReportId(expected_valid_report_response.Id);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_valid_report_response.ToString()}");


            //Assert
            Assert.Equal(expected_valid_report_response, actual_valid_report_response);

        }


        #endregion

        #region GetAllReports

        #endregion

        #region GetReportByReportId

        #endregion

        #region UpdateReport

        #endregion

        #region DeleteReportByReportId

        #endregion
    }
}
