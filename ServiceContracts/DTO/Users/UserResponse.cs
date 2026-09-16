using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ServiceContracts.DTO.Users
{
    /// <summary>
    /// This is the DTO return object of a UserAddRequest OR UserUpdateRequest
    /// </summary>
    public class UserResponse
    {

        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public Role Role { get; set; }

        public UserState UserState { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? ReceiveNewsLetter { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is UserResponse response &&
                   UserId.Equals(response.UserId) &&
                   Name == response.Name &&
                   Email == response.Email &&
                   Password == response.Password &&
                   DateOfBirth == response.DateOfBirth &&
                   Role == response.Role &&
                   UserState == response.UserState &&
                   ReceiveNewsLetter == response.ReceiveNewsLetter;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return $"User ID:{UserId}, Name:{Name}, Email:{Email}, Password:{Password}, Role:{Role}, User State: {UserState}, Date Of Birth: {DateOfBirth}, Receive News Letter: {ReceiveNewsLetter}";
        }


        public UpdateUserRequest ToUpdateUserRequest()
        {
            return new UpdateUserRequest()
            {
                UserId = this.UserId,
                Name = this.Name,
                Email = this.Email,
                Password = this.Password,
                ConfirmPassword = this.Password,
                Role = this.Role,
                UserState = this.UserState,
                DateOfBirth = this.DateOfBirth,
                ReceiveNewsLetter = this.ReceiveNewsLetter
            };
        }
    }

    public static class UserAddResponseExtension
    {
        public static UserResponse ToUserAddResponse(this User user)
        {

            return new UserResponse()
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
                UserState= user.UserState,
                DateOfBirth = user.DateOfBirth,
                ReceiveNewsLetter = user.ReceiveNewsLetter
            };
        }
    }



}
