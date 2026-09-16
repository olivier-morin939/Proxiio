using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Entities;
using Entities.Enums;

namespace ServiceContracts.DTO.Users
{
    /// <summary>
    /// This is the required DTO parameter object to update an already existing object of User Entity
    /// </summary>
    /// <returns>
    /// Returns an DTO object of type UserResponse
    /// </returns>
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "User Id can't be blank")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Name can't be blank")]
        [StringLength(120, ErrorMessage = "Name has to be between 1 and 120 char long", MinimumLength = 1)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email can't be blank")]
        [StringLength(254, ErrorMessage = "Email must be between 3 and 254 characters", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "Email should be in a valid format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password can't be blank")]
        [StringLength(255, ErrorMessage = "Password has to be between 8 and 255 char long", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,255}$", ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public Role Role { get; set; }

        public UserState UserState { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public bool? ReceiveNewsLetter { get; set; }

        public User ToUser()
        {
            return new User()
            {
                UserId = this.UserId,
                Name = this.Name,
                Email = this.Email,
                Password = this.Password,
                Role = this.Role,
                UserState = this.UserState,
                DateOfBirth = this.DateOfBirth,
                ReceiveNewsLetter = this.ReceiveNewsLetter
            };
        }


    }




}
