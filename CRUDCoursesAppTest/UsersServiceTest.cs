using Entities;
using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our UsersService
    /// </summary>
    public class UsersServiceTest
    {
        private readonly IUsersService _usersService;
        private readonly ITestOutputHelper _outputHelper;
        public UsersServiceTest(ITestOutputHelper outputHelper)
        {
            _usersService = new UsersService();
            _outputHelper = outputHelper;
        }

        public AddUserRequest AddBasicUserRequest()
        {
            return new AddUserRequest()
            {
                Name = "John Smith",
                Email = "johnsmith1234@gmail.com",
                Password = "Password1234!",
                ConfirmPassword = "Password1234!",
                Role = Role.User,
                UserState = UserState.Active,
                DateOfBirth = DateTime.Parse("2000-08-06"),
                ReceiveNewsLetter = true
            };
        }

        public UpdateUserRequest UpdateBasicUserRequest()
        {
            return new UpdateUserRequest()
            {
                Name = "John Smith",
                Email = "johnsmith1234@gmail.com",
                Password = "Password1234!",
                ConfirmPassword = "Password1234!",
                Role = Role.User,
                UserState = UserState.Active,
                DateOfBirth = DateTime.Parse("2000-08-06"),
                ReceiveNewsLetter = true
            };
        }

        public List<UserResponse> CopyOfMockData()
        {
            List<UserResponse> users_add_response_to_seed_data = new List<UserResponse>()
            {
                new UserResponse()
                {
                    UserId = Guid.Parse("5E1077F1-D7AE-4FA5-8202-571E7820B957"),
                    Name = "John Smith",
                    Email = "johnsmith@gmail.com",
                    Password = "Password1234!",
                    Role = Role.User,
                    UserState = UserState.Active,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("024B32AA-1AF0-4667-B60D-4F68ED55C46C"),
                    Name = "Lucifer Morningstar",
                    Email = "samael@gmail.com",
                    Password = "Password1234!",
                    Role = Role.Administrator,
                    UserState = UserState.Banned,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = true,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("7C0FFF05-AA1F-41F6-88D8-4CAEDD821AA1"),
                    Name = "Abigail Smoothy",
                    Email = "abigailsmoothy@gmail.com",
                    Role = Role.Moderator,
                    UserState = UserState.Inactive,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false
                }
            };
                return users_add_response_to_seed_data;
            
        }

        #region SeedUsersMockTesting
        [Fact]
        public void SeedUsersMockTesting()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> users_add_response_copy_of_mock_data = CopyOfMockData();
           

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse userReponseFromMockData in users_add_response_copy_of_mock_data)
            {
                _outputHelper.WriteLine($"{userReponseFromMockData.ToString()}");
            }

            //Act
            List<UserResponse> users_response_from_get = _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userReponseFromGet in users_response_from_get)
            {
                _outputHelper.WriteLine($"{userReponseFromGet.ToString()}");
            }

            //Assert
            foreach (UserResponse userReponseFromMockData in users_add_response_copy_of_mock_data)
            {
                Assert.Contains(userReponseFromMockData, users_response_from_get);
            }
        }

        [Fact]
        public void Concurrent_AddUser_SameEmail_OnlyOneCreated()
        {
            var svc = new Services.UsersService();
            string email = "concurrent@test.com";
            int attempts = 8;

            var tasks = Enumerable.Range(0, attempts).Select(i => Task.Run(() =>
            {
                try
                {
                    var req = new AddUserRequest()
                    {
                        Name = $"User{i}",
                        Email = email,
                        Password = "Password1234!",
                        ConfirmPassword = "Password1234!",
                        Role = Entities.Enums.Role.User,
                        UserState = Entities.Enums.UserState.Active,
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = false
                    };
                    svc.AddUser(req);
                    return true;
                }
                catch (DuplicateNameException)
                {
                    return false;
                }
            })).ToArray();

            Task.WaitAll(tasks);

            var results = tasks.Select(t => t.Result).ToList();
            var successes = results.Count(r => r);

            var all = svc.GetAllUsers().Where(u => u.Email == email).ToList();
            Assert.Equal(1, all.Count);
            Assert.Equal(1, successes);
        }

        [Fact]
        public void Concurrent_UpdateUser_EmailConflict_OneSucceeds()
        {
            var svc = new Services.UsersService();
            svc.SeedMockUsers();
            var users = svc.GetAllUsers();
            if (users.Count < 2)
            {
                svc.AddUser(new AddUserRequest() { Name = "A", Email = "a@test.com", Password = "Password1234!", ConfirmPassword = "Password1234!", Role = Entities.Enums.Role.User, UserState = Entities.Enums.UserState.Active, DateOfBirth = DateTime.UtcNow.AddYears(-30) });
                svc.AddUser(new AddUserRequest() { Name = "B", Email = "b@test.com", Password = "Password1234!", ConfirmPassword = "Password1234!", Role = Entities.Enums.Role.User, UserState = Entities.Enums.UserState.Active, DateOfBirth = DateTime.UtcNow.AddYears(-30) });
                users = svc.GetAllUsers();
            }

            var u1 = users[0];
            var u2 = users[1];
            string targetEmail = "target_concurrent@test.com";

            var upd1 = new UpdateUserRequest() { UserId = u1.UserId, Name = u1.Name, Email = targetEmail, Password = "Password1234!", ConfirmPassword = "Password1234!", Role = u1.Role, UserState = u1.UserState, DateOfBirth = u1.DateOfBirth, ReceiveNewsLetter = u1.ReceiveNewsLetter };
            var upd2 = new UpdateUserRequest() { UserId = u2.UserId, Name = u2.Name, Email = targetEmail, Password = "Password1234!", ConfirmPassword = "Password1234!", Role = u2.Role, UserState = u2.UserState, DateOfBirth = u2.DateOfBirth, ReceiveNewsLetter = u2.ReceiveNewsLetter };

            var t1 = Task.Run(() => { try { svc.UpdateUser(upd1); return true; } catch (DuplicateNameException) { return false; } });
            var t2 = Task.Run(() => { try { svc.UpdateUser(upd2); return true; } catch (DuplicateNameException) { return false; } });

            Task.WaitAll(t1, t2);

            var results = new[] { t1.Result, t2.Result };
            var successes = results.Count(r => r);
            var found = svc.GetAllUsers().Where(u => u.Email == targetEmail).ToList();

            Assert.Equal(1, found.Count);
            Assert.True(successes >= 1);
        }
    
        #endregion

        #region AddUser
        [Fact]
        public void AddUser_EmptyObject()
        {
            //Arrange
            AddUserRequest? new_user_add_null_request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() => {

                //Act
                _usersService.AddUser(new_user_add_null_request);

            });

        }

        [Fact]
        public void AddUser_EmptyProperties()
        {
            //Arrange
            List<AddUserRequest> new_user_null_properties = new List<AddUserRequest>()
            {
                AddBasicUserRequest(),
                AddBasicUserRequest(),
                AddBasicUserRequest(),
                AddBasicUserRequest()
            };
            new_user_null_properties[0].Name = null;
            new_user_null_properties[1].Email = null;
            new_user_null_properties[2].Password = null;
            new_user_null_properties[3].ConfirmPassword = null;

            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();

            //Assert
            foreach (AddUserRequest addEmptyPropertyUserRequest in new_user_null_properties)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    //Act
                    user_responses_from_add_request.Add(_usersService.AddUser(addEmptyPropertyUserRequest));
                });
            }

        }

        [Fact]
        public void AddUser_ValidateProperties()
        {
            //Arrange
            List<AddUserRequest> new_user_invalid_properties_add_request = new List<AddUserRequest>()
            {
                AddBasicUserRequest(),
                AddBasicUserRequest(),
                AddBasicUserRequest()
            };
            new_user_invalid_properties_add_request[0].Name = @"John SmithJohn SmithJohn SmithJohn SmithJohn Smith
                                                                John SmithJohn SmithJohn SmithJohn SmithJohn Smith
                                                                John SmithJohn SmithJohn SmithJohn SmithJohn Smith";
            new_user_invalid_properties_add_request[1].Email = "something.com";
            new_user_invalid_properties_add_request[2].Password = "password";

            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();

            //Assert
            foreach (AddUserRequest addInvalidPropertyUserRequest in new_user_invalid_properties_add_request)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    //Act
                    user_responses_from_add_request.Add(_usersService.AddUser(addInvalidPropertyUserRequest));
                });
            }

        }

        [Fact]
        public void AddUser_DuplicateUser()
        {
            //Arrange
            List<AddUserRequest> new_user_duplicate_properties = new List<AddUserRequest>()
            {
                AddBasicUserRequest()
            };
            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();
            user_responses_from_add_request.Add(_usersService.AddUser(AddBasicUserRequest()));

            //Assert
            foreach (AddUserRequest addDuplicateUserRequest in new_user_duplicate_properties)
            {
                Assert.Throws<DuplicateNameException>(() =>
                {
                    //Act
                    user_responses_from_add_request.Add(_usersService.AddUser(addDuplicateUserRequest));
                });
            }
        }

        [Fact]
        public void AddUser_ValidObject()
        {
            //Arrange
            List<AddUserRequest> user_add_requests = new List<AddUserRequest>()
            { 
               AddBasicUserRequest(),
               AddBasicUserRequest(),
               AddBasicUserRequest(),

            };
            user_add_requests[0].Email = "johnsmith1234@mail.com";
            user_add_requests[1].Email = "johnsmith123456@mail.com";
            user_add_requests[2].Email = "johnsmith1654@mail.com";
            List<UserResponse> user_responses_from_add_request = new List<UserResponse>();



            //Act
            foreach(AddUserRequest userRequestToAdd in user_add_requests)
            {
                user_responses_from_add_request.Add(_usersService.AddUser(userRequestToAdd));
            }


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse userResponseFromAdd in user_responses_from_add_request)
            {
                _outputHelper.WriteLine($"{userResponseFromAdd.ToString()}");
            }



            List<UserResponse> user_responses_from_get_request = _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userResponseFromGet in user_responses_from_get_request)
            {
                _outputHelper.WriteLine($"{userResponseFromGet.ToString()}");
            }

            //Assert
            foreach (UserResponse userResponseFromAdd in user_responses_from_add_request)
            {
                Assert.Contains(userResponseFromAdd, user_responses_from_get_request);
            }
        }

        #endregion

        #region GetAllUsers

        [Fact]
        public void GetAllUsers_EmptyObject() {

            //Arrange & Act
            List<UserResponse>? users_from_get = _usersService.GetAllUsers();


            //Assert
            Assert.Empty(users_from_get);
            Assert.True(users_from_get.Count == 0);
        
        }



        [Fact]
        public void GetAllUsers_FullObject()
        {

            //Arrange 
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse userResponsesFromMock in user_responses_from_mock_data)
            {
                _outputHelper.WriteLine($"{userResponsesFromMock.ToString()}");
            }

            //Act
            List<UserResponse>? users_from_get = _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userResponsesFromGet in users_from_get)
            {
                _outputHelper.WriteLine($"{userResponsesFromGet.ToString()}");
            }


            //Assert
            foreach (UserResponse userResponseFromMockData in user_responses_from_mock_data)
            {
                Assert.Contains(userResponseFromMockData, users_from_get);
            }

        }

        #endregion

        #region GetUserByUserId

        [Fact]
        public void GetUserByUserId_IdDoesNotExist()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();


            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                UserResponse actual_user_response = _usersService.GetUserById(Guid.NewGuid());
            });
        }

        [Fact]
        public void GetUserByUserId_ValidId()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();
            UserResponse expected_user_response = user_responses_from_mock_data[1];

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_user_response.ToString()}");

            //Act
            UserResponse actual_user_response = _usersService.GetUserById(user_responses_from_mock_data[1].UserId);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_user_response.ToString()}");


            //Assert
            Assert.Equal(expected_user_response, actual_user_response);
        }


        #endregion

        #region GetFilteredUsers

        // If the search text is null it should return the all users
        [Fact]
        public void GetFilteredUsers_EmptySearchText()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> user_responses_from_filtered_get = _usersService.GetFilteredUsers(nameof(User.Name), "");

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
            }

        }


        // It should return the matching person
        [Fact]
        public void GetFilteredUsers_SearchByName()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if (expectedUserResponse.Name != null)
                {
                    if (expectedUserResponse.Name.Contains("john", StringComparison.OrdinalIgnoreCase))
                    {
                        _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                    }
                }

            }


            //Act
            List<UserResponse> user_responses_from_filtered_get = _usersService.GetFilteredUsers(nameof(User.Name), "john");

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if(expectedUserResponse.Name != null)
                {
                    if (expectedUserResponse.Name.Contains("john", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
                    }
                }
            }

        }


        // It should return the matching role
        [Fact]
        public void GetFilteredUsers_SearchByRole()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
 
               if (expectedUserResponse.Role.ToString().Contains(Role.User.ToString(), StringComparison.OrdinalIgnoreCase))
               {
                   _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
               }
                

            }


            //Act
            List<UserResponse> user_responses_from_filtered_get = _usersService.GetFilteredUsers(nameof(User.Role), Role.User.ToString());

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                if (expectedUserResponse.Role.ToString().Contains(Role.User.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
                }
                
            }

        }


        // It should return the matching email
        [Fact]
        public void GetFilteredUsers_SearchByEmail()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if (expectedUserResponse.Email != null)
                {
                    if (expectedUserResponse.Email.Contains("john", StringComparison.OrdinalIgnoreCase))
                    {
                        _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                    }
                }

            }


            //Act
            List<UserResponse> user_responses_from_filtered_get = _usersService.GetFilteredUsers(nameof(User.Email), "john");

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if (expectedUserResponse.Email != null)
                {
                    if (expectedUserResponse.Email.Contains("john", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
                    }
                }
            }

        }



        // It should return the matching date of birth
        [Fact]
        public void GetFilteredUsers_SearchByDateOfBirth()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            DateTime filteredDate = user_responses_from_mock[0].DateOfBirth.Value;


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if (expectedUserResponse.DateOfBirth != null || expectedUserResponse.DateOfBirth.HasValue)
                {
                    if (expectedUserResponse.DateOfBirth.Value.ToString("dd MMM yyyy").Contains(filteredDate.ToString("dd MMM yyyy"), StringComparison.OrdinalIgnoreCase))
                    {
                        _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
                    }
                }
                    
            }


            //Act
            List<UserResponse> user_responses_from_filtered_get = _usersService.GetFilteredUsers(nameof(User.DateOfBirth), filteredDate.ToString("dd MMM yyyy"));

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_filtered_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                if (expectedUserResponse.DateOfBirth != null || expectedUserResponse.DateOfBirth.HasValue)
                {
                    if(expectedUserResponse.DateOfBirth.Value.ToString("dd MMM yyyy").Contains(filteredDate.ToString("dd MMM yyyy"), StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
                    }
                }
            }

        }



        #endregion

        #region GetSortedUsers

        // It should return the list of UserResponse sorted by Name in Ascending order
        [Fact]
        public void GetSortedUsers_SortByNameAsc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderBy(temp => temp.Name).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Name), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);
                
            }

        }


        // It should return the list of UserResponse sorted by Name in Descending order
        [Fact]
        public void GetSortedUsers_SortByNameDesc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderByDescending(temp => temp.Name).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Name), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Email in Ascending order
        [Fact]
        public void GetSortedUsers_SortByEmailAsc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderBy(temp => temp.Email).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Email), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Email in Descending order
        [Fact]
        public void GetSortedUsers_SortByEmailDesc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderByDescending(temp => temp.Email).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Email), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Role in Ascending order
        [Fact]
        public void GetSortedUsers_SortByRoleAsc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderBy(temp => temp.Role.ToString()).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Role), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Role in Descending order
        [Fact]
        public void GetSortedUsers_SortByRoleDesc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderByDescending(temp => temp.Role.ToString()).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.Role), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }

        // It should return the list of UserResponse sorted by Date of Birth in Ascending order
        [Fact]
        public void GetSortedUsers_SortByDateOfBirthAsc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderBy(temp => temp.DateOfBirth).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.DateOfBirth), SortOption.ASC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Date of Birth in Descending order
        [Fact]
        public void GetSortedUsers_SortByDateOfBirthDesc()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock = CopyOfMockData();
            user_responses_from_mock.OrderByDescending(temp => temp.DateOfBirth).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {
                _outputHelper.WriteLine($"{expectedUserResponse.ToString()}");
            }


            //Act
            List<UserResponse> all_user_responses = _usersService.GetAllUsers();
            List<UserResponse> user_responses_from_sorted_get = _usersService.GetSortedUsers(all_user_responses, nameof(User.DateOfBirth), SortOption.DESC);

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse actualUserResponse in user_responses_from_sorted_get)
            {
                _outputHelper.WriteLine($"{actualUserResponse.ToString()}");
            }

            //Assert
            foreach (UserResponse expectedUserResponse in user_responses_from_mock)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        #endregion

        #region UpdateUser

        [Fact]
        public void UpdateUser_EmptyObject()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();

            UpdateUserRequest? new_user_add_null_request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() => {

                //Act
                _usersService.UpdateUser(new_user_add_null_request);

            });

        }

        [Fact]
        public void UpdateUser_EmptyProperties()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();
            List<UpdateUserRequest> new_user_update_empty_prop_request = new List<UpdateUserRequest>()
            {
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest()
            };
            new_user_update_empty_prop_request[0].UserId = user_responses_from_mock_data[0].UserId;
            new_user_update_empty_prop_request[1].UserId = user_responses_from_mock_data[1].UserId;
            new_user_update_empty_prop_request[2].UserId = user_responses_from_mock_data[2].UserId;
            new_user_update_empty_prop_request[3].UserId = user_responses_from_mock_data[2].UserId;

            new_user_update_empty_prop_request[0].Name = null;
            new_user_update_empty_prop_request[1].Email = null;
            new_user_update_empty_prop_request[2].Password = null;
            new_user_update_empty_prop_request[3].ConfirmPassword = null;

            List<UserResponse> user_responses_from_update_requests = new List<UserResponse>();


            //Assert
            foreach(UpdateUserRequest userInvalidUpdateRequest in new_user_update_empty_prop_request)
            {
                Assert.Throws<ArgumentException>(() => {

                    //Act
                    user_responses_from_update_requests.Add(_usersService.UpdateUser(userInvalidUpdateRequest));

                });
            }


        }

        [Fact]
        public void UpdateUser_ValidateProperties()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();
            List<UpdateUserRequest> new_user_update_empty_prop_request = new List<UpdateUserRequest>()
            {
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest()
            };
            new_user_update_empty_prop_request[0].UserId = user_responses_from_mock_data[0].UserId;
            new_user_update_empty_prop_request[1].UserId = user_responses_from_mock_data[1].UserId;
            new_user_update_empty_prop_request[2].UserId = user_responses_from_mock_data[2].UserId;
            new_user_update_empty_prop_request[3].UserId = user_responses_from_mock_data[2].UserId;

            new_user_update_empty_prop_request[0].Name = @"nullNullnullNullnullNullnullNullnullNullnullNull
                                                           nullNullnullNullnullNullnullNullnullNullnullNull
                                                           nullNullnullNullnullNullnullNullnullNullnullNull
                                                           nullNullnullNullnullNullnullNullnullNullnullNull";
            new_user_update_empty_prop_request[1].Email = "invalidmail.com";
            new_user_update_empty_prop_request[2].Password = "password1234";
            new_user_update_empty_prop_request[2].ConfirmPassword = "password1234";
            new_user_update_empty_prop_request[3].Password = "Password1234!";
            new_user_update_empty_prop_request[3].ConfirmPassword = "Yassword1234!";

            List<UserResponse> user_responses_from_update_requests = new List<UserResponse>();

            //Assert
            foreach (UpdateUserRequest userInvalidUpdateRequest in new_user_update_empty_prop_request)
            {
                Assert.Throws<ArgumentException>(() => {

                    //Act
                    user_responses_from_update_requests.Add(_usersService.UpdateUser(userInvalidUpdateRequest));

                });
            }

        }

        [Fact]
        public void UpdateUser_DuplicateUser()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();
            UpdateUserRequest new_user_update_invalid_request = UpdateBasicUserRequest();
            new_user_update_invalid_request.UserId = user_responses_from_mock_data[2].UserId;
            new_user_update_invalid_request.Email = user_responses_from_mock_data[0].Email;


            //Assert
            Assert.Throws<DuplicateNameException>(() =>
            {
                //Act
                UserResponse invalid_user_response_from_update_request = _usersService.UpdateUser(new_user_update_invalid_request);
            });

        }

        [Fact]
        public void UpdateUser_ValidObject()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();
            UpdateUserRequest valid_user_update_request = UpdateBasicUserRequest();
            valid_user_update_request.UserId = user_responses_from_mock_data[0].UserId;


            //Act
            UserResponse expected_updated_user_response = _usersService.UpdateUser(valid_user_update_request);

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_updated_user_response.ToString()}");


            UserResponse actual_response_from_get = _usersService.GetUserById(expected_updated_user_response.UserId);

            
            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_response_from_get.ToString()}");
            

            //Assert
            Assert.Equal(expected_updated_user_response, actual_response_from_get);
            
        }


        #endregion

        #region DeleteUserByUserId
        [Fact]
        public void DeleteUserByUserId_DoesNotExist()
        {
            //Arrange
            _usersService.SeedMockUsers();


            //Act
            bool isDeleted = _usersService.DeleteUser(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
           
        }

        [Fact]
        public void DeleteUserByUserId_DoExist()
        {
            //Arrange
            _usersService.SeedMockUsers();
            List<UserResponse> user_responses_from_mock_data = CopyOfMockData();

            //Act
            bool isDeleted = _usersService.DeleteUser(user_responses_from_mock_data[0].UserId);

            //Assert
            Assert.True(isDeleted);

            
        }
        #endregion
    }
}
