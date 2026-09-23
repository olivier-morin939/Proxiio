using System;
using Entities.Enums;


namespace Entities
{
    /// <summary>
    ///  Represente la table ComunityMember
    /// </summary>
    public class ComunityMember
    {
        public Guid ComunityId { get; set; }

        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public ComunityRole Role { get; set; } = ComunityRole.Member;
    }
}
