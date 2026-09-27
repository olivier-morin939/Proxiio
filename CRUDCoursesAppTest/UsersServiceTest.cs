using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using Services;
using System.Data;
using Xunit.Abstractions;
using EntityFrameworkCoreMock;
using Moq;
using AutoFixture;
using FluentAssertions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our UsersService
    /// </summary>
    public class UsersServiceTest
    {
        private readonly IUsersService _usersService;
        private readonly Mock<IEncryptionsService> _encryptionsServiceMock;
        private readonly ITestOutputHelper _outputHelper;

        private readonly IFixture _fixture;
        public UsersServiceTest(ITestOutputHelper outputHelper)
        {
            _fixture = new Fixture();
            _outputHelper = outputHelper;
            _encryptionsServiceMock = new Mock<IEncryptionsService>();
            var usersInitialData = new List<User>() { };
            DbContextMock<ApplicationDbContext> appDbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
            var appDbContext = appDbContextMock.Object;
            appDbContextMock.CreateDbSetMock(temp => temp.Users, usersInitialData);
            _usersService = new UsersService(appDbContext, _encryptionsServiceMock.Object);
        }


        #region AddUser
        [Fact]
        public async Task AddUser_EmptyObject()
        {
            // Arrange
            AddUserRequest? new_user_add_null_request = null;

            // Prepared Act
            Func<Task> action = async () =>
            {
                UserResponse invalid_user_response = await _usersService.AddUser(new_user_add_null_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        [Fact]
        public async Task AddUser_EmptyProperties()
        {

            //Arrange 
            AddUserRequest new_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Name, null as string)
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, null as string)
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, null as string)
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request4 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, null as string)
            .Create();

            List<AddUserRequest> invalid_user_add_request = new List<AddUserRequest>() { new_user_request1, new_user_request2, new_user_request3, new_user_request4 };
            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest addEmptyPropertyUserRequest in invalid_user_add_request)
                {
                    user_responses_from_add_request.Add(await _usersService.AddUser(addEmptyPropertyUserRequest));
                }
                    
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();


        }

        [Fact]
        public async Task AddUser_ValidateProperties()
        {

            //Arrange 
            AddUserRequest new_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Name, @"John SmithJohn SmithJohn SmithJohn SmithJohn Smith
                                       John SmithJohn SmithJohn SmithJohn SmithJohn Smith
                                       John SmithJohn SmithJohn SmithJohn SmithJohn Smith")
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, "password")
            .With(temp => temp.ConfirmPassword, "password")
            .Create();

            List<AddUserRequest> invalid_user_add_request = new List<AddUserRequest>() { new_user_request1, new_user_request2, new_user_request3 };
            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();


            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest addInvalidPropertyUserRequest in invalid_user_add_request)
                {
                    user_responses_from_add_request.Add(await _usersService.AddUser(addInvalidPropertyUserRequest));
                }
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();


        }

        [Fact]
        public async Task AddUser_DuplicateUser()
        {
            //Arrange
            AddUserRequest valid_new_user_request = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest duplicate_new_user_request = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            List<AddUserRequest> new_user_duplicate_properties = new List<AddUserRequest>() { duplicate_new_user_request };
            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();
            user_responses_from_add_request.Add(await _usersService.AddUser(valid_new_user_request));

            // Prepared Act
            Func <Task> action = async () =>
            {
                foreach (AddUserRequest addDuplicateUserRequest in new_user_duplicate_properties)
                {
                    user_responses_from_add_request.Add(await _usersService.AddUser(addDuplicateUserRequest));
                }
            };

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();

           
        }

        [Fact]
        public async Task AddUser_ValidObject()
        {
            // Arrange
            AddUserRequest new_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "example@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            AddUserRequest new_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@email.com")
            .With(temp => temp.Password, "ValidPassword1234!")
            .With(temp => temp.ConfirmPassword, "ValidPassword1234!")
            .Create();

            List<AddUserRequest> user_add_requests = new List<AddUserRequest>() { new_user_request1 , new_user_request2, new_user_request3};
            List <UserResponse> user_responses_from_add_request = new List<UserResponse>();

            // Prepared Act
            Func<Task> action = async () =>
            {
                
                foreach (AddUserRequest userRequestToAdd in user_add_requests)
                {
                    user_responses_from_add_request.Add(await _usersService.AddUser(userRequestToAdd));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse userResponseFromAdd in user_responses_from_add_request)
                {
                    _outputHelper.WriteLine($"{userResponseFromAdd.ToString()}");
                }
            };

            // Act
            await action.Invoke();

            List<UserResponse> user_responses_from_get_request = await _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userResponseFromGet in user_responses_from_get_request)
            {
                // Helper
                _outputHelper.WriteLine($"{userResponseFromGet.ToString()}");
                
                // Assert
                user_responses_from_get_request.Should().Contain(userResponseFromGet);
            }


        }

        #endregion

        #region GetAllUsers

        [Fact]
        public async Task GetAllUsers_EmptyObject()
        {

            //Arrange & Act
            List<UserResponse>? users_from_get = await _usersService.GetAllUsers();


            //Assert
            Assert.Empty(users_from_get);
            Assert.True(users_from_get.Count == 0);

        }

        [Fact]
        public async Task GetAllUsers_FullObject()
        {

            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();



            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest userRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(userRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse userResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{userResponse.ToString()}");
                }

            };

            //Act
            await action.Invoke();
            List<UserResponse> users_from_get = await _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userResponsesFromGet in users_from_get)
            {
                // Helper
                _outputHelper.WriteLine($"{userResponsesFromGet.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(userResponsesFromGet);

            }



        }

        #endregion

        #region GetUserByUserId

        [Fact]
        public async Task GetUserByUserId_IdDoesNotExist()
        {


            // Prepared Act & Arrange
            Func<Task> action = async () =>
            {
                UserResponse invalid_user_response = await _usersService.GetUserById(Guid.NewGuid());
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetUserByUserId_ValidId()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();


            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach(AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                _outputHelper.WriteLine($"{user_response_from_add[0].ToString()}");
            };

            // Act
            await action.Invoke();
            UserResponse actual_user_response = await _usersService.GetUserById(user_response_from_add[0].UserId);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_user_response.ToString()}");

            // Assert
            user_response_from_add[0].Should().Be(actual_user_response);
        }


        #endregion

        #region GetFilteredUsers

        // If the search text is null it should return the all users
        [Fact]
        public async Task GetFilteredUsers_EmptySearchText()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();


            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest userRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(userRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };

            //Act
            await action.Invoke();
            List<UserResponse> user_responses_from_filtered_get = await _usersService.GetFilteredUsers(nameof(User.Name), "");

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

           

        }


        // It should return the matching person
        [Fact]
        public async Task GetFilteredUsers_SearchByName()
        {
            // Arrange
            AddUserRequest user1 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Name, "John Doe")
                .With(temp => temp.Email, "something@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user2 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Name, "Jane Smith")
                .With(temp => temp.Email, "hello@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user3 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Name, "Johnny English") 
                .With(temp => temp.Email, "other@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            List<AddUserRequest> requests = new List<AddUserRequest> { user1, user2, user3 };
            List<UserResponse> expectedUsers = new List<UserResponse>();

            foreach (AddUserRequest request in requests)
            {
                UserResponse response = await _usersService.AddUser(request);
                if (response.Name != null && response.Name.Contains("john", StringComparison.OrdinalIgnoreCase))
                {
                    expectedUsers.Add(response);
                }
            }

            // Act
            List<UserResponse> actualUsers = await _usersService.GetFilteredUsers(nameof(User.Name), "john");

            // Logs
            _outputHelper.WriteLine("Expected:");
            expectedUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));
            _outputHelper.WriteLine("\nActual:");
            actualUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));

            // Assert
            actualUsers.Should().BeEquivalentTo(expectedUsers);
        }


        // It should return the matching role
        [Fact]
        public async Task GetFilteredUsers_SearchByRole()
        {
            // Arrange
            AddUserRequest user1 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "johndoe@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user2 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "janesmith@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user3 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "johnyenglish@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            List<AddUserRequest> requests = new List<AddUserRequest> { user1, user2, user3 };
            List<UserResponse> expectedUsers = new List<UserResponse>();

            foreach (AddUserRequest request in requests)
            {
                UserResponse response = await _usersService.AddUser(request);
                if (response.Role.ToString() != null && response.Role.ToString().Contains(Role.User.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    expectedUsers.Add(response);
                }
            }

            // Act
            List<UserResponse> actualUsers = await _usersService.GetFilteredUsers(nameof(User.Role), Role.User.ToString());

            // Logs
            _outputHelper.WriteLine("Expected:");
            expectedUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));
            _outputHelper.WriteLine("\nActual:");
            actualUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));

            // Assert
            actualUsers.Should().BeEquivalentTo(expectedUsers);

        }


        // It should return the matching email
        [Fact]
        public async Task GetFilteredUsers_SearchByEmail()
        {
            // Arrange
            AddUserRequest user1 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "johndoe@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user2 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "janesmith@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user3 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "johnyenglish@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            List<AddUserRequest> requests = new List<AddUserRequest> { user1, user2, user3 };
            List<UserResponse> expectedUsers = new List<UserResponse>();

            foreach (AddUserRequest request in requests)
            {
                UserResponse response = await _usersService.AddUser(request);
                if (response.Email != null && response.Email.Contains("john", StringComparison.OrdinalIgnoreCase))
                {
                    expectedUsers.Add(response);
                }
            }

            // Act
            List<UserResponse> actualUsers = await _usersService.GetFilteredUsers(nameof(User.Email), "john");

            // Logs
            _outputHelper.WriteLine("Expected:");
            expectedUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));
            _outputHelper.WriteLine("\nActual:");
            actualUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));

            // Assert
            actualUsers.Should().BeEquivalentTo(expectedUsers);

        }


        // It should return the matching date of birth
        [Fact]
        public async Task GetFilteredUsers_SearchByDateOfBirth()
        {
            // Arrange
            DateTime targetDate = new DateTime(2000, 2, 23);
            AddUserRequest user1 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "something@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .With(temp => temp.DateOfBirth, targetDate)
                .Create();

            AddUserRequest user2 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "otherthing@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .Create();

            AddUserRequest user3 = _fixture.Build<AddUserRequest>()
                .With(temp => temp.Email, "hello@example.com")
                .With(temp => temp.Password, "TestingPassword1234!")
                .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
                .With(temp => temp.DateOfBirth, targetDate)
                .Create();

            List<AddUserRequest> requests = new List<AddUserRequest> { user1, user2, user3 };
            List<UserResponse> expectedUsers = new List<UserResponse>();

            foreach (AddUserRequest request in requests)
            {
                UserResponse response = await _usersService.AddUser(request);
                if (response.DateOfBirth.HasValue && response.DateOfBirth.Value.Date.Equals(targetDate.Date))
                {
                    expectedUsers.Add(response);
                }
            }

            // Act
            List<UserResponse> actualUsers = await _usersService.GetFilteredUsers(nameof(User.DateOfBirth), targetDate.ToString("dd MMM yyyy"));

            // Logs
            _outputHelper.WriteLine("Expected:");
            expectedUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));
            _outputHelper.WriteLine("\nActual:");
            actualUsers.ForEach(u => _outputHelper.WriteLine(u.ToString()));

            // Assert
            actualUsers.Should().BeEquivalentTo(expectedUsers);

        }



        #endregion

        #region GetSortedUsers

        // It should return the list of UserResponse sorted by Name in Ascending order
        [Fact]
        public async Task GetSortedUsers_SortByNameAsc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderBy(temp => temp.Name).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach(AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Name), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }



        }


        // It should return the list of UserResponse sorted by Name in Descending order
        [Fact]
        public async Task GetSortedUsers_SortByNameDesc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderByDescending(temp => temp.Name).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Name), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }



        }


        // It should return the list of UserResponse sorted by Email in Ascending order
        [Fact]
        public async Task GetSortedUsers_SortByEmailAsc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderBy(temp => temp.Email).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Email), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }


        }


        // It should return the list of UserResponse sorted by Email in Descending order
        [Fact]
        public async Task GetSortedUsers_SortByEmailDesc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderByDescending(temp => temp.Email).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Email), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

        }


        // It should return the list of UserResponse sorted by Role in Ascending order
        [Fact]
        public async Task GetSortedUsers_SortByRoleAsc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderBy(temp => temp.Role.ToString()).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Role), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

        }


        // It should return the list of UserResponse sorted by Role in Descending order
        [Fact]
        public async Task GetSortedUsers_SortByRoleDesc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderByDescending(temp => temp.Role.ToString()).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.Role), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

        }

        // It should return the list of UserResponse sorted by Date of Birth in Ascending order
        [Fact]
        public async Task GetSortedUsers_SortByDateOfBirthAsc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderBy(temp => temp.DateOfBirth?.ToString("dd MMM yyyy") ?? string.Empty).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.DateOfBirth), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

        }


        // It should return the list of UserResponse sorted by Date of Birth in Descending order
        [Fact]
        public async Task GetSortedUsers_SortByDateOfBirthDesc()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            user_response_from_add.OrderByDescending(temp => temp.DateOfBirth?.ToString("dd MMM yyyy") ?? string.Empty).ToList();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (AddUserRequest fullAddRequest in full_user_requests)
                {
                    user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
                }

                _outputHelper.WriteLine("Expected:");
                foreach (UserResponse expectedUserResponse in user_response_from_add)
                {
                    _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                }
            };


            //Act
            await action.Invoke();
            List<UserResponse> all_user_responses = await _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = await _usersService.GetSortedUsers(all_user_responses, nameof(User.DateOfBirth), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                // Helper
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");

                // Assert
                user_response_from_add.Should().Contain(actualUserResponse);
            }

        }


        #endregion

        #region UpdateUser

        [Fact]
        public async Task UpdateUser_EmptyObject()
        {

            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            UpdateUserRequest? new_user_add_null_request = null;

            foreach (AddUserRequest userRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(userRequest));
            }


            // Prepared Act
            Func<Task> action = async () =>
            {
                await _usersService.UpdateUser(new_user_add_null_request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateUser_EmptyProperties()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request4 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "another@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3, full_user_request4 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            foreach(AddUserRequest fullAddRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
            }



            UpdateUserRequest emptyprop_update_request1 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[0].UserId)
            .With(temp => temp.Name, null as string)
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            UpdateUserRequest emptyprop_update_request2 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[1].UserId)
            .With(temp => temp.Email, null as string)
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            UpdateUserRequest emptyprop_update_request3 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[2].UserId)
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, null as string)
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            UpdateUserRequest emptyprop_update_request4 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[3].UserId)
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, null as string)
            .Create();

            List<UpdateUserRequest> emptyprops_update_requests = new List<UpdateUserRequest>() { emptyprop_update_request1, emptyprop_update_request2, emptyprop_update_request3, emptyprop_update_request4 };
            List<UserResponse> user_responses_from_update_requests = new List<UserResponse>();


            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (UpdateUserRequest userInvalidUpdateRequest in emptyprops_update_requests)
                {
                    user_responses_from_update_requests.Add(await _usersService.UpdateUser(userInvalidUpdateRequest));
                }
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task UpdateUser_ValidateProperties()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request4 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "another@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3, full_user_request4 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            foreach (AddUserRequest fullAddRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
            }

            UpdateUserRequest emptyprop_update_request1 = _fixture.Build<UpdateUserRequest>()
             .With(temp => temp.UserId, user_response_from_add[0].UserId)
             .With(temp => temp.Name, @"nullNullnullNullnullNullnullNullnullNullnullNull
                                        nullNullnullNullnullNullnullNullnullNullnullNull
                                        nullNullnullNullnullNullnullNullnullNullnullNull
                                        nullNullnullNullnullNullnullNullnullNullnullNull")
             .With(temp => temp.Email, "something@example.com")
             .With(temp => temp.Password, "TestingPassword1234!")
             .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
             .Create();

            UpdateUserRequest emptyprop_update_request2 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[1].UserId)
            .With(temp => temp.Email, "invalidmail.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            UpdateUserRequest emptyprop_update_request3 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[2].UserId)
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "password1234")
            .With(temp => temp.ConfirmPassword, "password1234")
            .Create();


            UpdateUserRequest emptyprop_update_request4 = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[3].UserId)
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, null as string)
            .Create();

            List<UpdateUserRequest> emptyprops_update_requests = new List<UpdateUserRequest>() { emptyprop_update_request1, emptyprop_update_request2, emptyprop_update_request3, emptyprop_update_request4 };
            List<UserResponse> user_responses_from_update_requests = new List<UserResponse>();

            // Prepared Act
            Func<Task> action = async () =>
            {
                foreach (UpdateUserRequest userInvalidUpdateRequest in emptyprops_update_requests)
                {
                    user_responses_from_update_requests.Add(await _usersService.UpdateUser(userInvalidUpdateRequest));
                }
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task UpdateUser_DuplicateUser()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request4 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "another@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3, full_user_request4 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            foreach (AddUserRequest fullAddRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
            }

            UpdateUserRequest duplicate_user = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[0].UserId)
            .With(temp => temp.Email, user_response_from_add[1].Email)
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            // Prepared Act
            Func<Task> action = async () =>
            {
                UserResponse invalid_user_response_from_update_request = await _usersService.UpdateUser(duplicate_user);
            };

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();

        }

        [Fact]
        public async Task UpdateUser_ValidObject()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request4 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "another@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3, full_user_request4 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
            foreach (AddUserRequest fullAddRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(fullAddRequest));
            }


            UpdateUserRequest valid_user_update_request = _fixture.Build<UpdateUserRequest>()
            .With(temp => temp.UserId, user_response_from_add[0].UserId)
            .With(temp => temp.Email, "newemail@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();
            List<UserResponse> user_reponse_from_update = new List<UserResponse>();

            // Prepared Act
            Func<Task> action = async () =>
            {
                user_reponse_from_update.Add( await _usersService.UpdateUser(valid_user_update_request));

                _outputHelper.WriteLine("Expected:");
                _outputHelper.WriteLine($"{user_reponse_from_update[0].ToString()}");
            };

            //Act
            await action.Invoke();

            UserResponse actual_response_from_get = await _usersService.GetUserById(user_reponse_from_update[0].UserId);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_response_from_get.ToString()}");

            //Assert
            actual_response_from_get.Should().Be(user_reponse_from_update[0]);

        }


        #endregion

        #region DeleteUserByUserId
        [Fact]
        public async Task DeleteUserByUserId_DoesNotExist()
        {

            //Arrange
            bool isDeleted = false;

            Func<Task> action = async () =>
            {
                isDeleted = await _usersService.DeleteUser(Guid.NewGuid());
            };

            // Act
            await action.Invoke();

            //Assert
            isDeleted.Should().BeFalse();

        }

        [Fact]
        public async Task DeleteUserByUserId_DoExist()
        {
            //Arrange 
            AddUserRequest full_user_request1 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "something@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            AddUserRequest full_user_request2 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "hello@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();


            AddUserRequest full_user_request3 = _fixture.Build<AddUserRequest>()
            .With(temp => temp.Email, "other@example.com")
            .With(temp => temp.Password, "TestingPassword1234!")
            .With(temp => temp.ConfirmPassword, "TestingPassword1234!")
            .Create();

            List<AddUserRequest> full_user_requests = new List<AddUserRequest>() { full_user_request1, full_user_request2, full_user_request3 };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in full_user_requests)
            {
                user_response_from_add.Add(await _usersService.AddUser(userRequest));
            }

            bool isDeleted = false;


            Func<Task> action = async () =>
            {
                isDeleted = await _usersService.DeleteUser(user_response_from_add[0].UserId); ;
            };


            //Act
            await action.Invoke();

            //Assert
            isDeleted.Should().BeTrue();


        }
        #endregion
    }
}