using System;
using Entities.Enums;


namespace Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public Role Role { get; set; }

        public string? Password { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? ReceiveNewsLetter { get; set; }
    }
}
