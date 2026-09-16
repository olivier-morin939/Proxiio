using System;

namespace ServiceContracts.DTO.Posts
{
    public class PostLikeResponse
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
