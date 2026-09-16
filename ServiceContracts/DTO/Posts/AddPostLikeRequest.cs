using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO.Posts
{
    public class AddPostLikeRequest
    {
        [Required]
        public Guid PostId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
