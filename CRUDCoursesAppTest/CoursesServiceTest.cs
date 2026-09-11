using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Services.Helpers;
using System.Data;
using Xunit;
using Xunit.Abstractions;
namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our CoursesService
    /// </summary>
    public class CoursesServiceTest
    {
        private readonly ICoursesService _coursesService;
        private readonly ITestOutputHelper _outputHelper;
        public CoursesServiceTest(ITestOutputHelper outputHelper)
        {
            _coursesService = new CoursesService();
            _outputHelper = outputHelper;
        }

        #region SeedMockCoursesTesting
        [Fact]
        public void SeedMockCourses_ReturnValidCourses()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_to_seed_data = new List<CourseResponse>()
            {
                new CourseResponse() { CourseId = _coursesService.GetAllCourses()[0].CourseId, CourseName = "C++ professionnal developpement", CourseDescription = "professionnal c++ course." },
                new CourseResponse() { CourseId = _coursesService.GetAllCourses()[1].CourseId, CourseName = "C# professionnal developpement", CourseDescription = "professionnal c# course." },
                new CourseResponse() { CourseId = _coursesService.GetAllCourses()[2].CourseId, CourseName = "Python professionnal developpement", CourseDescription = "professionnal Python course." }

            };
            _outputHelper.WriteLine("Expected:");
            foreach (CourseResponse courseReponseFromMockData in courses_add_response_to_seed_data)
            {
                _outputHelper.WriteLine($"{courseReponseFromMockData.ToString()}");
            }

            //Act
            List<CourseResponse> courses_response_from_get = _coursesService.GetAllCourses();

            _outputHelper.WriteLine("Actual:");
            foreach(CourseResponse courseReponseFromGet in courses_response_from_get)
            {
                _outputHelper.WriteLine($"{courseReponseFromGet.ToString()}");
            }

            //Assert
            foreach(CourseResponse courseReponseFromGet in courses_response_from_get)
            {
                Assert.Contains(courseReponseFromGet, courses_add_response_to_seed_data);
            }

        }
        #endregion

        #region AddInvalidCourses
        [Fact]
        public void AddCourse_ObjectIsNull()
        {

            //Arrange
            CourseAddRequest? courseAddRequest = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _coursesService.AddCourse(courseAddRequest);
            });


        }

        [Fact]
        public void AddCourse_CourseNameIsNull()
        {

            //Arrange
            CourseAddRequest? courseAddRequest = new CourseAddRequest()
            { CourseName = null , CourseDescription = "Dummy description"};

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _coursesService.AddCourse(courseAddRequest);
            });


        }


        [Fact]
        public void AddCourse_CourseNameIsInvalid()
        {
            //Arrange
            CourseAddRequest? courseAddRequest = new CourseAddRequest()
            { CourseName = "Dummy course nameDummy course nameDummy course nameDummy course nameDummy course nameDummy course nameDummy course nameDummy course name", CourseDescription = "Dummy course description" };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                HelpersValidation.ModelValidation(courseAddRequest);
                _coursesService.AddCourse(courseAddRequest);
            });


        }

        [Fact]
        public void AddCourse_CourseDescriptionIsNull()
        {

            //Arrange
            CourseAddRequest? courseAddRequest = new CourseAddRequest()
            { CourseName = "Dummy course name", CourseDescription = null };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _coursesService.AddCourse(courseAddRequest);
            });


        }

        [Fact]
        public void AddCourse_CourseDescriptionIsInvalid() {

            //Arrange
            CourseAddRequest? courseAddRequest = new CourseAddRequest()
            { 
                CourseName = "Dummy course name", 
                CourseDescription = "Dummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course description" 
            };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                HelpersValidation.ModelValidation(courseAddRequest);
                _coursesService.AddCourse(courseAddRequest);
            });

        }

        [Fact]
        public void AddCourse_CourseNameIsDuplicate()
        {

            //Arrange
            CourseAddRequest? courseAddRequest1 = new CourseAddRequest()
            { CourseName = "Dummy course name", CourseDescription = "Dummy course description" };
            CourseAddRequest? courseAddRequest2 = new CourseAddRequest()
            { CourseName = "Dummy course name", CourseDescription = "Dummy course description" };

            //Assert
            Assert.Throws<DuplicateNameException>(() =>
            {
                //Act
                _coursesService.AddCourse(courseAddRequest1);
                _coursesService.AddCourse(courseAddRequest2);
            });


        }

        [Fact]
        public void AddCourse_CourseParamsIsInvalid() {


            //Arrange
            CourseAddRequest? courseAddRequest = new CourseAddRequest()
            {
                CourseName = "Dummy course name Dummy course name Dummy course name Dummy course name Dummy course name Dummy course name Dummy course name Dummy course name",
                CourseDescription = "Dummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course descriptionDummy course description"
            };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                HelpersValidation.ModelValidation(courseAddRequest);
                _coursesService.AddCourse(courseAddRequest);
            });

        }
        #endregion

        #region AddValidCourses
        [Fact]
        public void AddCourse_ValidObject()
        {
            //Arrange
            List<CourseAddRequest> course_add_requests = new List<CourseAddRequest>()
            {
                new CourseAddRequest() {CourseName = "Dummy course name1", CourseDescription = "Dummy course description1"},
                new CourseAddRequest() {CourseName = "Dummy course name2", CourseDescription = "Dummy course description2"},
                new CourseAddRequest() {CourseName = "Dummy course name3", CourseDescription = "Dummy course description3"}

            };

            //Act
            List<CourseResponse> course_response_from_add_requests = new List<CourseResponse>();
            foreach(CourseAddRequest courseAddRequest in course_add_requests)
            {
                HelpersValidation.ModelValidation(courseAddRequest);
                course_response_from_add_requests.Add(_coursesService.AddCourse(courseAddRequest));
            }
            List<CourseResponse> course_response_from_get = _coursesService.GetAllCourses();

            _outputHelper.WriteLine("Expected:");
            foreach(CourseResponse countryResponseFromGet in course_response_from_get)
            {
                _outputHelper.WriteLine($"{countryResponseFromGet.ToString()}");
            }


            //Assert
            _outputHelper.WriteLine("Actual:");
            foreach (CourseResponse courseResponseFromGet in course_response_from_get)
            {
                _outputHelper.WriteLine($"{courseResponseFromGet.ToString()}");
                Assert.True(courseResponseFromGet.CourseId != Guid.Empty);
                Assert.Contains(courseResponseFromGet, course_response_from_add_requests);
            }

        }
        #endregion

        #region GetAllInvalidCourses
        [Fact]
        public void GetAllCourses_ReturnsZeroWhenNoCoursesExist()
        {

            //Act
            List<CourseResponse>? courseReponse = _coursesService.GetAllCourses();

            //Assert
            Assert.True(courseReponse.Count() == 0);
            Assert.Empty(courseReponse);


        }
        #endregion

        #region GetAllValidCourses
        [Fact]
        public void GetAllCourses_ReturnsAllCourses()
        {

            //Arrange
            List<CourseAddRequest> courses_add_request = new List<CourseAddRequest>()
            { 
                new CourseAddRequest(){ CourseName = "Dummy course name", CourseDescription = "Dummy description"},
                new CourseAddRequest(){ CourseName = "Dummy course name2", CourseDescription = "Dummy description2"},
                new CourseAddRequest(){ CourseName = "Dummy course name3", CourseDescription = "Dummy description3"}
            };
            List<CourseResponse> courses_add_responses_from_add_request = new List<CourseResponse>();



            //Act
            foreach(CourseAddRequest courseAddRequest in courses_add_request)
            {
                courses_add_responses_from_add_request.Add(_coursesService.AddCourse(courseAddRequest));
            }


            List<CourseResponse>? course_add_responses_from_get = _coursesService.GetAllCourses();

            _outputHelper.WriteLine("Actual:");
            foreach(CourseResponse courseReponseFromGet in course_add_responses_from_get)
            {
                _outputHelper.WriteLine(courseReponseFromGet.ToString());
            }


            //Assert
            Assert.True(course_add_responses_from_get.Count() > 0);
            foreach(CourseResponse courseReponseFromGet in course_add_responses_from_get)
            {
                _outputHelper.WriteLine(courseReponseFromGet.ToString());
                Assert.True(courseReponseFromGet.CourseId != Guid.Empty);
                Assert.Contains(courseReponseFromGet, courses_add_responses_from_add_request);
            }


        }
        #endregion

        #region GetCourseByIdInvalidCourse
        [Fact]
        public void GetCourseById_ReturnsNullWhenNoCourseExists()
        {

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                CourseResponse? courseReponse = _coursesService.GetCourseById(Guid.NewGuid());
            });

        }
        #endregion

        #region GetCourseByIdValidCourse
        [Fact]
        public void GetCourseById_ReturnsValidCourseResponse()
        {
            //Arrange
            List<CourseAddRequest> courses_add_request = new List<CourseAddRequest>()
            { 
                new CourseAddRequest(){ CourseName = "Dummy course name1",CourseDescription = "Dummy description1"},
                new CourseAddRequest(){ CourseName = "Dummy course name2",CourseDescription = "Dummy description2"}
            };
            List<CourseResponse> courses_add_responses_from_add_request = new List<CourseResponse>();

            //Act
            foreach(CourseAddRequest courseAddRequest in courses_add_request)
            {
                courses_add_responses_from_add_request.Add(_coursesService.AddCourse(courseAddRequest));
            }
            List<CourseResponse> courses_add_response_from_get = _coursesService.GetAllCourses();
            CourseResponse returned_course_response = courses_add_responses_from_add_request[0];
            CourseResponse targeted_course_response_from_get = _coursesService.GetCourseById(courses_add_response_from_get[0].CourseId);
            
            
            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{returned_course_response.ToString()}");

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{targeted_course_response_from_get.ToString()}");


            //Assert
            Assert.Equal(targeted_course_response_from_get, returned_course_response);


        }
        #endregion

        #region UpdateCourseByIdInvalidCourse
        [Fact]
        public void UpdateCourse_CourseDoesNotExist()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_responses_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = Guid.NewGuid(),
                CourseName = "Dummy Course",
                CourseDescription = "Dummy description"
            };

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                CourseResponse course_response_from_update = _coursesService.UpdateCourse(course_update_request);
            });

        }

        [Fact]
        public void UpdateCourse_ObjectIsNull()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_responses_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest? course_update_request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                CourseResponse course_response_from_update = _coursesService.UpdateCourse(course_update_request);
            });

        }

        [Fact]
        public void UpdateCourse_CourseNameIsNull()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = null,
                CourseDescription = "Dummy course description"

            };

            //Assert
            Assert.Throws<ArgumentException>(() => {

                //Act
                CourseResponse course_response_from_update_request = _coursesService.UpdateCourse(course_update_request);
            
            });


        }

        [Fact]
        public void UpdateCourse_CourseNameValidation()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz",
                CourseDescription = "Dummy course description"

            };

            //Assert
            Assert.Throws<ArgumentException>(() => {

                //Act
                CourseResponse course_response_from_update_request = _coursesService.UpdateCourse(course_update_request);

            });
        }

        [Fact]
        public void UpdateCourse_CourseDescriptionIsNull()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = "Dummy course name",
                CourseDescription = null

            };
            CourseResponse targeted_course_response = _coursesService.GetCourseById(courses_add_response_from_mock_data[0].CourseId);
            targeted_course_response.CourseName = course_update_request.CourseName;


            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{targeted_course_response.ToString()}");


            //Act
            CourseResponse updated_course_response = _coursesService.UpdateCourse(course_update_request);
            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{updated_course_response.ToString()}");

            //Assert
            Assert.Equal(targeted_course_response, updated_course_response);
        }


        [Fact]
        public void UpdateCourse_CourseDescriptionValidation()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = "dumme course name",
                CourseDescription = @"zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
                                       zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz"

            };

            //Assert
            Assert.Throws<ArgumentException>(() => {

                //Act
                CourseResponse course_response_from_update_request = _coursesService.UpdateCourse(course_update_request);

            });
        }

        [Fact]
        public void UpdateCourse_CourseNameDuplicate()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request1 = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = "Dummy course name",
                CourseDescription = null

            };
            CourseUpdateRequest course_update_request2 = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[1].CourseId,
                CourseName = "Dummy course name",
                CourseDescription = null

            };


            //Assert
            Assert.Throws<DuplicateNameException>(() =>
            {
                //Act
                CourseResponse valid_update_response = _coursesService.UpdateCourse(course_update_request1);
                CourseResponse invalid_update_response = _coursesService.UpdateCourse(course_update_request2);
            });

        }

        #endregion

        #region UpdateCourseByIdValidCourse
        [Fact]
        public void UpdateCourseById_ValidModification()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> courses_add_response_from_mock_data = _coursesService.GetAllCourses();
            CourseUpdateRequest course_update_request = new CourseUpdateRequest()
            {
                CourseId = courses_add_response_from_mock_data[0].CourseId,
                CourseName = "Dummy course name",
                CourseDescription = "Some new information"

            };
            CourseResponse? targeted_course_response = new CourseResponse()
            {
                CourseId = course_update_request.CourseId,
                CourseName = course_update_request.CourseName,
                CourseDescription = course_update_request.CourseDescription
            };

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{targeted_course_response.ToString()}");

            //Act

            CourseResponse valid_course_response_from_update_request = _coursesService.UpdateCourse(course_update_request);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{valid_course_response_from_update_request.ToString()}");

            //Assert
            Assert.Equal(valid_course_response_from_update_request, targeted_course_response);

        }

        #endregion

        #region DeleteCourseByIdInvalidCourse
        [Fact]
        public void DeleteCourseById_CourseDoesNotExist()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> course_response_from_mock_data = _coursesService.GetAllCourses();

            //Act
            bool? isDeleted = _coursesService.DeleteCourseById(Guid.NewGuid());
            List<CourseResponse> course_response_after_deletion = _coursesService.GetAllCourses();


            //Assert
            Assert.True(isDeleted == false);
            foreach(CourseResponse courseResponseFromGet in course_response_after_deletion)
            {
                Assert.Contains(courseResponseFromGet, course_response_from_mock_data);
            }


        }

        #endregion

        #region DeleteCourseByIdValidCourse
        [Fact]
        public void DeleteCourseById_CourseExist()
        {
            //Arrange
            _coursesService.SeedMockCourses();
            List<CourseResponse> course_response_from_mock_data = _coursesService.GetAllCourses();

            //Act
            bool? isDeleted = _coursesService.DeleteCourseById(course_response_from_mock_data[0].CourseId);
            List<CourseResponse> course_response_after_deletion = _coursesService.GetAllCourses();


            //Assert
            Assert.True(isDeleted == true);
            Assert.True(course_response_from_mock_data.Count() > course_response_after_deletion.Count());
           

        }

        #endregion
    }
}
