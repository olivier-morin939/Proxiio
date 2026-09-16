using System;
using System.Collections.Generic;
using Entities;

namespace ServiceContracts.DTO.Posts
{
    public class PostResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public List<string>? ImagesPath { get; set; } = new List<string>();

        public List<string>? AdditionalsPath { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }

        public int LikesCount { get; set; }

        public int CommentsCount { get; set; }

    }

    public static class PostResponseExtensions
    {
        public static PostResponse ToPostResponse(this Post post)
        {
            return new PostResponse()
            {
                Id = post.Id,
                UserId = post.UserId,
                Title = post.Title,
                Body = post.Body,
                ImagesPath = post.ImagesPath ?? new List<string>(),
                AdditionalsPath = post.AdditionalsPath ?? new List<string>(),
                CreatedAt = post.CreatedAt,
                LikesCount = post.Likes?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0
            };
        }
    }
}
