using System;
using Entities;

namespace ServiceContracts.DTO.Posts
{
    public static class PostLikeExtensions
    {
        public static PostLikeResponse ToPostLikeResponse(this PostLike like)
        {
            return new PostLikeResponse()
            {
                PostId = like.PostId,
                UserId = like.UserId,
                LikedAt = like.LikedAt
            };
        }
    }
}
