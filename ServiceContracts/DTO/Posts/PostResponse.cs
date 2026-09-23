using Entities;
using Entities.Enums;
using ServiceContracts.DTO.Users;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ServiceContracts.DTO.Posts
{
    public class PostResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public List<string>? ImagesPath { get; set; } = new List<string>();

        public List<string>? AdditionalsPath { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }

        public int ReportsCount { get; set; }


        public override string ToString()
        {
            return $"Post Id: {this.Id}, User Id: {this.UserId}, Title: {this.Title}, Body : {this.Body}, Images Count: {this.ImagesPath?.Count}, Additional Files Count: {this.AdditionalsPath?.Count}, Created At: {this.CreatedAt.ToString("dd MMM yyyy")}, Modified At: {this.ModifiedAt.ToString("dd MMM yyyy")}";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is PostResponse response &&
                   Id.Equals(response.Id) &&
                   UserId.Equals(response.UserId) &&
                   Title == response.Title &&
                   Body == response.Body &&
                   ImagesPath?.Count == response.ImagesPath?.Count &&
                   AdditionalsPath?.Count == response.AdditionalsPath?.Count &&
                   CreatedAt.Equals(response.CreatedAt) &&
                   ModifiedAt.Equals(response.ModifiedAt);
                
        }

    }

    public static class PostResponseExtensions
    {
        public static PostResponse ToPostResponse(this Post post)
        {
            return new PostResponse()
            {
                Id = post.Id,
                UserId = post.UserId,
                CommunityId = post.CommunityId,
                Title = post.Title,
                Body = post.Body,
                ImagesPath = post.ImagesPath ?? new List<string>(),
                AdditionalsPath = post.AdditionalsPath ?? new List<string>(),
                CreatedAt = post.CreatedAt
            };
        }
    }
}
