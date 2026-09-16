using System;
using Entities;

namespace ServiceContracts.DTO.Comunities
{
    public static class ComunityMemberExtensions
    {
        public static ComunityMemberResponse ToComunityMemberResponse(this ComunityMember member)
        {
            return new ComunityMemberResponse()
            {
                ComunityId = member.ComunityId,
                UserId = member.UserId,
                JoinedAt = member.JoinedAt,
                Role = member.Role
            };
        }
    }
}
