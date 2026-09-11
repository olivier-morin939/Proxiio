using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Entities;
using Entities.Enums;

namespace ServiceContracts.DTO
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
        public string? Name { get; set; }

        public string? Email { get; set; }

        public Role Role { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }

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
                DateOfBirth = this.DateOfBirth,
                ReceiveNewsLetter = this.ReceiveNewsLetter
            };
        }


    }




}
