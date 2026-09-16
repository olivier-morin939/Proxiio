using System;
using Entities.Enums;

namespace ServiceContracts.DTO.Comunities
{
    public class ComunityMemberResponse
    {
        public Guid ComunityId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public ComunityRole Role { get; set; }
    }
}
