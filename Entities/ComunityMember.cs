using System;
using Entities.Enums;

namespace Entities
{
    public class ComunityMember
    {
        public Guid ComunityId { get; set; }
        public Comunity? Comunity { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public ComunityRole Role { get; set; } = ComunityRole.Member;
    }
}
