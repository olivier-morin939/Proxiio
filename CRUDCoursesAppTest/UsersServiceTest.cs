using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
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
                Role = Entities.Enums.Role.User,
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
                Role = Entities.Enums.Role.User,
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
                    Role = Entities.Enums.Role.User,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("024B32AA-1AF0-4667-B60D-4F68ED55C46C"),
                    Name = "Lucifer Morningstar",
                    Email = "samael@gmail.com",
                    Password = "Password1234!",
                    Role = Entities.Enums.Role.Administrator,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = true,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("7C0FFF05-AA1F-41F6-88D8-4CAEDD821AA1"),
                    Name = "Abigail Smoothy",
                    Email = "abigailsmoothy@gmail.com",
                    Role = Entities.Enums.Role.Moderator,
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
            List<UserResponse> users_add_response_to_seed_data = new List<UserResponse>()
            {
                new UserResponse()
                {
                    UserId = Guid.Parse("5E1077F1-D7AE-4FA5-8202-571E7820B957"),
                    Name = "John Smith",
                    Email = "johnsmith@gmail.com",
                    Password = "Password1234!",
                    Role = Entities.Enums.Role.User,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("024B32AA-1AF0-4667-B60D-4F68ED55C46C"),
                    Name = "Lucifer Morningstar",
                    Email = "samael@gmail.com",
                    Password = "Password1234!",
                    Role = Entities.Enums.Role.Administrator,
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = true,

                },
                new UserResponse()
                {

                    UserId = Guid.Parse("7C0FFF05-AA1F-41F6-88D8-4CAEDD821AA1"),
                    Name = "Abigail Smoothy",
                    Email = "abigailsmoothy@gmail.com",
                    Role = Entities.Enums.Role.Moderator,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false
                },
            };


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse userReponseFromMockData in users_add_response_to_seed_data)
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
            foreach (UserResponse userReponseFromMockData in users_add_response_to_seed_data)
            {
                Assert.Contains(userReponseFromMockData, users_response_from_get);
            }
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
            new_user_update_empty_prop_request[3].UserId = user_responses_from_mock_data[3].UserId;

            new_user_update_empty_prop_request[0].Name = null;
            new_user_update_empty_prop_request[1].Email = null;
            new_user_update_empty_prop_request[2].Password = null;
            new_user_update_empty_prop_request[3].ConfirmPassword = null;




            //Assert
            Assert.Throws<ArgumentNullException>(() => {

                //Act
                _usersService.UpdateUser(new_user_add_null_request);

            });

        }

        [Fact]
        public void UpdateUser_ValidateProperties()
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
        public void UpdateUser_DuplicateUser()
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
        public void UpdateUser_ValidObject()
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
            foreach (AddUserRequest userRequestToAdd in user_add_requests)
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

        #region DeleteUserByUserId

        #endregion
    }
}
