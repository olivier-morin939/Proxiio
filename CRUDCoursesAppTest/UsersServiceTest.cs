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

        public AddUserRequest AddBasicUserRequest
        (
            string Name = "John Smith",
            string Email = "johnsmith1234@gmail.com",
            string Password = "Password1234!",
            string ConfirmPassword = "Password1234!",
            Role Role = Role.User,
            UserState UserState = UserState.Active,
            string DateOfBirthStr = "2000-08-06",
            bool ReceiveNewsLetter = true
        )
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach(AddUserRequest userRequest in user_requests) 
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse userResponse in user_response_from_add)
            {
                _outputHelper.WriteLine($"{userResponse.ToString()}");
            }

            //Act
            List<UserResponse>? users_from_get = _usersService.GetAllUsers();

            _outputHelper.WriteLine("Actual:");
            foreach (UserResponse userResponsesFromGet in users_from_get)
            {
                _outputHelper.WriteLine($"{userResponsesFromGet.ToString()}");
            }


            //Assert
            foreach (UserResponse userResponseFromMockData in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                user_response_from_add.Add(_usersService.GetUserById(Guid.NewGuid()));
            });
        }

        [Fact]
        public void GetUserByUserId_ValidId()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();
           
            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            UserResponse expected_user_response = user_response_from_add[1];

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_user_response.ToString()}");

            //Act
            UserResponse actual_user_response = _usersService.GetUserById(user_response_from_add[1].UserId);

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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {
                Assert.Contains(expectedUserResponse, user_responses_from_filtered_get);
            }

        }


        // It should return the matching person
        [Fact]
        public void GetFilteredUsers_SearchByName()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }
            DateTime filteredDate = user_response_from_add[0].DateOfBirth.Value;


            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderBy(temp => temp.Name).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);
                
            }

        }


        // It should return the list of UserResponse sorted by Name in Descending order
        [Fact]
        public void GetSortedUsers_SortByNameDesc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderByDescending(temp => temp.Name).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Email in Ascending order
        [Fact]
        public void GetSortedUsers_SortByEmailAsc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderBy(temp => temp.Email).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Email in Descending order
        [Fact]
        public void GetSortedUsers_SortByEmailDesc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderByDescending(temp => temp.Email).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Role in Ascending order
        [Fact]
        public void GetSortedUsers_SortByRoleAsc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderBy(temp => temp.Role.ToString()).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Role in Descending order
        [Fact]
        public void GetSortedUsers_SortByRoleDesc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderByDescending(temp => temp.Role.ToString()).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }

        // It should return the list of UserResponse sorted by Date of Birth in Ascending order
        [Fact]
        public void GetSortedUsers_SortByDateOfBirthAsc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderBy(temp => temp.DateOfBirth).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
            {

                Assert.Contains(expectedUserResponse, user_responses_from_sorted_get);

            }

        }


        // It should return the list of UserResponse sorted by Date of Birth in Descending order
        [Fact]
        public void GetSortedUsers_SortByDateOfBirthDesc()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            user_response_from_add.OrderByDescending(temp => temp.DateOfBirth).ToList();

            _outputHelper.WriteLine("Expected:");
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            foreach (UserResponse expectedUserResponse in user_response_from_add)
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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com")
            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            List<UpdateUserRequest> new_user_update_empty_prop_request = new List<UpdateUserRequest>()
            {
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest()
            };
            new_user_update_empty_prop_request[0].UserId = user_response_from_add[0].UserId;
            new_user_update_empty_prop_request[1].UserId = user_response_from_add[1].UserId;
            new_user_update_empty_prop_request[2].UserId = user_response_from_add[2].UserId;
            new_user_update_empty_prop_request[3].UserId = user_response_from_add[2].UserId;

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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            List<UpdateUserRequest> new_user_update_empty_prop_request = new List<UpdateUserRequest>()
            {
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest(),
                UpdateBasicUserRequest()
            };
            new_user_update_empty_prop_request[0].UserId = user_response_from_add[0].UserId;
            new_user_update_empty_prop_request[1].UserId = user_response_from_add[1].UserId;
            new_user_update_empty_prop_request[2].UserId = user_response_from_add[2].UserId;
            new_user_update_empty_prop_request[3].UserId = user_response_from_add[2].UserId;

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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            UpdateUserRequest new_user_update_invalid_request = UpdateBasicUserRequest();
            new_user_update_invalid_request.UserId = user_response_from_add[2].UserId;
            new_user_update_invalid_request.Email = user_response_from_add[0].Email;


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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }

            UpdateUserRequest valid_user_update_request = UpdateBasicUserRequest();
            valid_user_update_request.UserId = user_response_from_add[0].UserId;


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
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }


            //Act
            bool isDeleted = _usersService.DeleteUser(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
           
        }

        [Fact]
        public void DeleteUserByUserId_DoExist()
        {
            //Arrange
            List<AddUserRequest> user_requests = new List<AddUserRequest>()
            {
                AddBasicUserRequest(Email:"testing1234@email.com"),
                AddBasicUserRequest(Email:"testing9084@email.com"),
                AddBasicUserRequest(Email:"testing9999@email.com"),
                AddBasicUserRequest(Email:"testing5555@email.com")

            };
            List<UserResponse> user_response_from_add = new List<UserResponse>();

            foreach (AddUserRequest userRequest in user_requests)
            {
                user_response_from_add.Add(_usersService.AddUser(userRequest));
            }
            //Act
            bool isDeleted = _usersService.DeleteUser(user_response_from_add[0].UserId);

            //Assert
            Assert.True(isDeleted);

            
        }
        #endregion
    }
}
