using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;


namespace Entities
{
    /// <summary>
    ///  Represente la table User
    /// </summary>
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [StringLength(120)]
        public string? Name { get; set; }

        [StringLength(254)]
        public string? Email { get; set; }

        public Role Role { get; set; }

        public UserState UserState { get; set; }

        [StringLength(255)]
        public string? Password { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? ReceiveNewsLetter { get; set; }
    }
}
