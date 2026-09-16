using Entities;
using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using System.Xml.Linq;

namespace Services
{
    public class UsersService : IUsersService
    {

        private readonly List<User> _users = new List<User>();
        private readonly object _lock = new object();

        public void SeedMockUsers()
        {
            _users.AddRange(
                new User() { 

                    UserId = Guid.Parse("5E1077F1-D7AE-4FA5-8202-571E7820B957"),
                    Name = "John Smith",
                    Email = "johnsmith@gmail.com",
                    Role = Role.User,
                    UserState = UserState.Active,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false
                
                },
                new User() {

                    UserId = Guid.Parse("024B32AA-1AF0-4667-B60D-4F68ED55C46C"),
                    Name = "Lucifer Morningstar",
                    Email = "samael@gmail.com",
                    Role = Role.Administrator,
                    UserState = UserState.Banned,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = true

                },
                new User() {

                    UserId = Guid.Parse("7C0FFF05-AA1F-41F6-88D8-4CAEDD821AA1"),
                    Name = "Abigail Smoothy",
                    Email = "abigailsmoothy@gmail.com",
                    Role = Role.Moderator,
                    UserState = UserState.Inactive,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false
                });
        }


        public UserResponse AddUser(AddUserRequest? addUserRequest)
        {
            // If the AddUserRequest is null
            if(addUserRequest == null)
            {
                throw new ArgumentNullException(nameof(addUserRequest));
            }

            // Declaring the properties in a dictionary with their name
            var fields = new Dictionary<string, string?>
            {
                { nameof(addUserRequest.Name), addUserRequest.Name },
                { nameof(addUserRequest.Email), addUserRequest.Email },
                { nameof(addUserRequest.Password), addUserRequest.Password },
                { nameof(addUserRequest.ConfirmPassword), addUserRequest.ConfirmPassword }
            };

            // Checking if the properties are null 
            foreach (var (fieldName, fieldValue) in fields)
            {
                if (string.IsNullOrEmpty(fieldValue))
                {
                    throw new ArgumentException(fieldName);
                }
            }


            // Checking if the user already exist and add atomically
            lock (_lock)
            {
                User? matchingUser = _users.Where(u => u.Email == addUserRequest.Email).FirstOrDefault();
                if (matchingUser != null)
                {
                    throw new DuplicateNameException(nameof(addUserRequest.Email));
                }

                // Model validation for the properties
                Helpers.HelpersValidation.ModelValidation(addUserRequest);

                // Checking if Password and ConfirmPassword match
                if (addUserRequest.Password != addUserRequest.ConfirmPassword)
                {
                    throw new ArgumentException("Passwords do not match.", nameof(addUserRequest.ConfirmPassword));
                }

                // Creating the new user as an User object
                User newUser = addUserRequest.ToUser();

                // Add the user to the list
                _users.Add(newUser);

                // Return the newly created user
                return newUser.ToUserAddResponse();
            }


        }


        public List<UserResponse> GetAllUsers()
        {
            if( _users == null || _users.Count == 0)
            {
                return new List<UserResponse>();
            } else
            {
                return _users.Select(c => c.ToUserAddResponse()).ToList();
            }
        }

        public UserResponse GetUserById(Guid userId)
        {
            if(userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            User? matchingUser = _users.Where(u => u.UserId == userId).FirstOrDefault();
            if (matchingUser == null) 
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return matchingUser.ToUserAddResponse();

        }


        public UserResponse UpdateUser(UpdateUserRequest? updateUserRequest)
        {
            // Check if the given object is null
            if(updateUserRequest == null)
            {
                throw new ArgumentNullException(nameof(updateUserRequest));
            }



            // perform find, duplicate check and update atomically
            lock (_lock)
            {
                // Check if the given object exist
                int matchingUserIndex = _users.FindIndex(u => u.UserId == updateUserRequest.UserId);
                User? matchingUser = matchingUserIndex >= 0 ? _users[matchingUserIndex] : null;
                if (matchingUser == null)
                {
                    throw new ArgumentNullException(nameof(updateUserRequest.UserId));
                }

                // Check if the given object email is already registered
                User? duplicateUser = _users.Where(u => u.UserId != updateUserRequest.UserId && u.Email == updateUserRequest.Email).FirstOrDefault();
                if (duplicateUser != null)
                {
                    throw new DuplicateNameException(nameof(duplicateUser.Email));
                }

                // Check if the password and confirm password match
                if (updateUserRequest.Password != updateUserRequest.ConfirmPassword)
                {
                    throw new ArgumentException(nameof(updateUserRequest.ConfirmPassword));
                }

                // Validate the informations of the model
                Helpers.HelpersValidation.ModelValidation(updateUserRequest);

                // Updating the informations of the object

                matchingUser.Name = updateUserRequest.Name;
                matchingUser.Email = updateUserRequest.Email;
                matchingUser.DateOfBirth = updateUserRequest.DateOfBirth;
                matchingUser.ReceiveNewsLetter = updateUserRequest.ReceiveNewsLetter;
                matchingUser.Password = updateUserRequest.Password;
                matchingUser.Role = updateUserRequest.Role;
                matchingUser.UserState = updateUserRequest.UserState;

                _users[matchingUserIndex] = matchingUser;

                // Return the updated object
                return matchingUser.ToUserAddResponse();
            }


        }

        public bool DeleteUser(Guid userId)
        {
            // Checking if the user id is not empty
            if(userId == Guid.Empty)
            {
                return false;
            }

            // Getting the targeted user
            int matchingUserIndex = _users.FindIndex(u => u.UserId == userId);
            if(matchingUserIndex == -1)
            {
                return false;
            }

            // Deleting the element
            User matchingUser = _users[matchingUserIndex];
            _users.RemoveAt(matchingUserIndex);

            return true;


        }

        public List<UserResponse> GetFilteredUsers(string searchBy, string searchString)
        {
            // Check if 'searchBy' is not null
            if (string.IsNullOrEmpty(searchBy))
            {
                return GetAllUsers();
            }

            // Check if 'searchContent' is not null
            if (string.IsNullOrEmpty(searchString))
            {
                return GetAllUsers();
            }


            List<User> filtered_users = new List<User>();

            // Get matching users from List<User> based on the properties and content
            switch (searchBy)
            {
                case nameof(UserResponse.Name):
                    filtered_users = _users.Where(user => user.Name != null && user.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(UserResponse.Role):
                    filtered_users = _users.Where(user => user.Role.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(UserResponse.UserState):
                    filtered_users = _users.Where(user => user.UserState.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(UserResponse.Email):
                    filtered_users = _users.Where(user => user.Email != null && user.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(UserResponse.DateOfBirth):
                    filtered_users = _users.Where(user => user.DateOfBirth.HasValue && user.DateOfBirth.Value.ToString("dd MMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

            }

            // Convert the matching users to UserResponses
            List<UserResponse> filtered_user_responses = filtered_users.Select(user => user.ToUserAddResponse()).ToList();

            // Return all UserResponses
            return filtered_user_responses;


        }

        public List<UserResponse> GetSortedUsers(List<UserResponse> allUserResponses, string sortBy, SortOption sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allUserResponses;
            }


            return sortBy switch
            {
                nameof(UserResponse.Name) => sortOrder == SortOption.ASC
                    ? allUserResponses.OrderBy(user => user.Name).ToList()
                    : allUserResponses.OrderByDescending(user => user.Name).ToList(),

                nameof(UserResponse.Role) => sortOrder == SortOption.ASC
                    ? allUserResponses.OrderBy(user => user.Role).ToList()
                    : allUserResponses.OrderByDescending(user => user.Role).ToList(),

                nameof(UserResponse.UserState) => sortOrder == SortOption.ASC
                    ? allUserResponses.OrderBy(user => user.UserState).ToList()
                    : allUserResponses.OrderByDescending(user => user.UserState).ToList(),

                nameof(UserResponse.Email) => sortOrder == SortOption.ASC
                    ? allUserResponses.OrderBy(user => user.Email).ToList()
                    : allUserResponses.OrderByDescending(user => user.Email).ToList(),

                nameof(UserResponse.DateOfBirth) => sortOrder == SortOption.ASC
                    ? allUserResponses.OrderBy(user => user.DateOfBirth).ToList()
                    : allUserResponses.OrderByDescending(user => user.DateOfBirth).ToList(),

                _ => allUserResponses
            };
        }

        public int GetAllUsersCount()
        {
            return _users.Count();
        }
    }
}
