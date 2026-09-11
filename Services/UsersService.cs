using Entities;
using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO;
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

        public void SeedMockUsers()
        {
            _users.Clear();
            _users.AddRange(
                new User() { 

                    UserId = Guid.Parse("5E1077F1-D7AE-4FA5-8202-571E7820B957"),
                    Name = "John Smith",
                    Email = "johnsmith@gmail.com",
                    Role = Role.User,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = false
                
                },
                new User() {

                    UserId = Guid.Parse("024B32AA-1AF0-4667-B60D-4F68ED55C46C"),
                    Name = "Lucifer Morningstar",
                    Email = "samael@gmail.com",
                    Role = Role.Administrator,
                    Password = "Password1234!",
                    DateOfBirth = DateTime.Parse("1990-06-23"),
                    ReceiveNewsLetter = true

                },
                new User() {

                    UserId = Guid.Parse("7C0FFF05-AA1F-41F6-88D8-4CAEDD821AA1"),
                    Name = "Abigail Smoothy",
                    Email = "abigailsmoothy@gmail.com",
                    Role = Role.Moderator,
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

            // Checking if the user already exist
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
            throw new NotImplementedException();
        }

        public bool DeleteUser(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
