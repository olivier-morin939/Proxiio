using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;

namespace ServiceContracts.DTO.Comunities
{
    public class ComunityResponse
    {
        public Guid Id { get; set; }

        public Guid TeacherId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public int UsersCount { get; set; }

        public int PostsCount { get; set; }

        public List<PostResponse>? Posts { get; set; } = new List<PostResponse>();

        public List<UserResponse>? Users { get; set; } = new List<UserResponse>();
    }

    public static class ComunityResponseExtensions
    {
        public static ComunityResponse ToComunityResponse(this Comunity com)
        {
            return new ComunityResponse()
            {
                Id = com.Id,
                TeacherId = com.TeacherId,
                UsersCount = com.Users?.Count ?? 0,
                PostsCount = com.PostsList?.Count ?? 0,
                Name = com.Name,
                Description = com.Description,
                Posts = com.PostsList?.Select(p => p.ToPostResponse()).ToList() ?? new List<PostResponse>(),
                Users = com.Users?.Select(u => u.ToUserAddResponse()).ToList() ?? new List<UserResponse>()
            };
        }
    }
}
