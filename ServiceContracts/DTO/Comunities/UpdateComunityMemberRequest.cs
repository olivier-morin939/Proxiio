using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;

namespace ServiceContracts.DTO.Comunities
{
    public class UpdateComunityMemberRequest
    {
        [Required]
        public Guid ComunityId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public ComunityRole Role { get; set; }
    }
}
